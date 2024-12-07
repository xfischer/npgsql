using pcap2latex.Model;
using pcap2latex.Templates;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Diagnostics;
using System.Text;

namespace pcap2latex;

public static class PcapService
{
    public static IEnumerable<PostgresPacket> ConvertPcap(string pcapFile, ushort pgsqlPortNumber = 5432)
    {
        using var device = new CaptureFileReaderDevice(pcapFile);
        device.Open(new());

        // Capture packets using GetNextPacket()
        PacketCapture e;
        byte[]? previousRemainder = null;
        int packetRelativeIndex = 0;
        while (device.GetNextPacket(out e) == GetPacketStatus.PacketRead)
        {
            var time = e.Header.Timeval.Date;
            var len = e.Data.Length;
            var rawPacket = e.GetPacket();

            var packet = PacketDotNet.Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
            var tcpPacket = packet.Extract<PacketDotNet.TcpPacket>();
            if (tcpPacket != null) // only TCP packets
            {
                var ipPacket = (PacketDotNet.IPPacket)tcpPacket.ParentPacket;
                System.Net.IPAddress srcIp = ipPacket.SourceAddress;
                System.Net.IPAddress dstIp = ipPacket.DestinationAddress;
                int srcPort = tcpPacket.SourcePort;
                int dstPort = tcpPacket.DestinationPort;

                if ((srcPort == pgsqlPortNumber || dstPort == pgsqlPortNumber) && tcpPacket.PayloadData.Length > 0) // only TCP packets on Postgres port with data
                {
                    Console.WriteLine("{0}:{1}:{2},{3} Len={4} {5}:{6} -> {7}:{8}",
                        time.Hour, time.Minute, time.Second, time.Millisecond, len,
                        srcIp, srcPort, dstIp, dstPort);

                    var pgPacket = ParsePacket(tcpPacket, ipPacket, pgsqlPortNumber, previousRemainder, out var remainder);
                    pgPacket.PacketIndex = packetRelativeIndex++;
                    yield return pgPacket;

                    previousRemainder = remainder;
                }
            }
        }
    }



    // Minumum viable PDML info
    private static PostgresPacket ParsePacket(PacketDotNet.TcpPacket tcpPacket, PacketDotNet.IPPacket ipPacket, ushort pgsqlPortNumber, byte[]? previousRemainder, out byte[]? nextRemainder)
    {
        var pgPacket = new PostgresPacket();

        // IPV6
        pgPacket.SourceAddress = ipPacket.SourceAddress;
        pgPacket.DestinationAddress = ipPacket.DestinationAddress;

        // TCP
        pgPacket.SourcePort = tcpPacket.SourcePort;
        pgPacket.DestinationPort = tcpPacket.DestinationPort;
        pgPacket.IsFrontEnd = tcpPacket.DestinationPort == pgsqlPortNumber;

        // Reconstruct packet
        byte[] payloadData = GetReconstructedPayload(tcpPacket, previousRemainder);
        using var memStream = new MemoryStream(payloadData);
        using var reader = new PcapBinaryReader(memStream, Encoding.UTF8);

        nextRemainder = null;
        while (reader.BaseStream.Position != memStream.Length)
        {
            try
            {
                var currentPosition = reader.BaseStream.Position;
                ushort clientPort = pgPacket.IsFrontEnd ? pgPacket.SourcePort : pgPacket.DestinationPort;
                if (TryReadMessage(reader, pgPacket.IsFrontEnd, (int)clientPort, out var message))
                {
                    Debug.Assert(message != null);
                    pgPacket.Messages.Add(message!);
                }
                else
                {

                    reader.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
                    nextRemainder = reader.ReadBytes((int)(memStream.Length - currentPosition));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex.ToString()}");
            }

        }

        return pgPacket;

        static byte[] GetReconstructedPayload(PacketDotNet.TcpPacket tcpPacket, byte[]? previousRemainder)
        {
            byte[] payloadData = tcpPacket.PayloadData;
            if (previousRemainder is not null)
            {
                payloadData = new byte[previousRemainder.Length + payloadData.Length];
                Array.Copy(previousRemainder, payloadData, previousRemainder.Length);
                Array.Copy(tcpPacket.PayloadData, 0, payloadData, previousRemainder.Length, tcpPacket.PayloadData.Length);
            }

            return payloadData;
        }
    }

    private static Dictionary<int, bool> _sslRequestedFromClient = new();
    private static bool SSLRequestedFromClient(int port)
    {
        if (_sslRequestedFromClient.TryGetValue(port, out var ssl))
            return ssl;
        return false;
    }
    private static Dictionary<int, AuthenticationGenericMessage?> _lastAuthPacket = new();


    private static AuthenticationGenericMessage? LastAuthPacket(int port)
    {
        if (_lastAuthPacket.TryGetValue(port, out var message))
            return message;
        return null;
    }
    private static void SetLastAuthPacket(int port, AuthenticationGenericMessage? message)
    {
        _lastAuthPacket[port] = message;
    }
    private static void SetSSLRequestedFromClient(int port, bool value)
    {
        _sslRequestedFromClient[port] = value;
    }

    private static bool TryReadMessage(PcapBinaryReader reader, bool isFrontEnd, int clientPort, out PostgresMessageBase? message)
    {
        try
        {
            var messageCode = reader.ReadChar();

            // Check if auth
            int messageLength = 0;
            if (messageCode == 0)
            {
                messageCode = '?';
                reader.BaseStream.Seek(-1, SeekOrigin.Current);
                messageLength = reader.ReadInt32();
            }

            var pgMessage = PostgresMessages.GetMessage(messageCode, isFrontEnd);
            if (pgMessage == null)
                throw new NotSupportedException($"Message not supported for code: '{messageCode}'");

            message = pgMessage.Value.Name! switch
            {
                nameof(Parse) => ParseMessage.Read(messageCode, reader),
                nameof(Bind) => BindMessage.Read(messageCode, reader),
                nameof(Describe) => DescribeMessage.Read(messageCode, reader),
                nameof(Execute) => ExecuteMessage.Read(messageCode, reader),
                nameof(ExecuteOut) => ExecuteOutMessage.Read(messageCode, reader),
                nameof(Sync) => SyncMessage.Read(messageCode, reader),
                nameof(Query) => QueryMessage.Read(messageCode, reader),
                nameof(ParseOut) => ParseOutMessage.Read(messageCode, reader),
                nameof(NoData) => NoDataMessage.Read(messageCode, reader),
                nameof(BindComplete) => BindCompleteMessage.Read(messageCode, reader),
                nameof(ParseComplete) => ParseCompleteMessage.Read(messageCode, reader),
                nameof(ParameterDescription) => ParameterDescriptionMessage.Read(messageCode, reader),
                nameof(RowDescription) => RowDescriptionMessage.Read(messageCode, reader),
                nameof(ReadyForQuery) => ReadyForQueryMessage.Read(messageCode, reader),
                nameof(DescribeOut) => DescribeOutMessage.Read(messageCode, reader),
                nameof(OutDescription) => OutDescriptionMessage.Read(messageCode, reader),
                nameof(DataRow) => DataRowMessage.Read(messageCode, reader),
                nameof(CommandComplete) => CommandCompleteMessage.Read(messageCode, reader),
                nameof(NoticeResponse) when SSLRequestedFromClient(clientPort) => SSLResponseMessage.Read(messageCode),
                nameof(NoticeResponse) => NoticeResponseMessage.Read(messageCode, reader),
                "ParamData" => SendOutTupleMessage.Read(messageCode, reader),
                nameof(Terminate) => TerminateMessage.Read(messageCode, reader),
                "StartupMessage" when messageLength == 8 => SSLRequestMessage.Read(messageCode, messageLength, reader),
                "StartupMessage" when messageLength > 8 => StartupMessageMessage.Read(messageCode, messageLength, reader),
                "AuthenticationRequest" => AuthenticationMessage.Read(messageCode, reader),
                "Password" => LastAuthPacket(clientPort)!.ReadResponseMessage(messageCode, reader),
                nameof(ParameterStatus) => ParameterStatusMessage.Read(messageCode, reader),
                nameof(BackendKeyData) => BackendKeyDataMessage.Read(messageCode, reader),
                nameof(ErrorResponse) => ErrorResponseMessage.Read(messageCode, reader),
                _ => null,
            };

            if (message == null)
                throw new NotSupportedException($"Message not supported for code: '{messageCode}'");

            if (message is AuthenticationGenericMessage authMsg)
            {
                SetLastAuthPacket(clientPort, authMsg);
            }

            SetSSLRequestedFromClient(clientPort, message is SSLRequestMessage);

            return true;
        }
        catch (EndOfStreamException ex)
        {
            message = null;
            return false;
        }
    }
}
