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
        PcapReadState state = new();
        state.Port = pgsqlPortNumber;

        using var device = new CaptureFileReaderDevice(pcapFile);
        device.Open(new());

        // Capture packets using GetNextPacket()
        PacketCapture e;
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

                    var pgPacket = ParsePacket(tcpPacket, ipPacket, state);
                    pgPacket.PacketIndex = packetRelativeIndex++;
                    yield return pgPacket;

                }
            }
        }
    }


    private static PostgresPacket ParsePacket(PacketDotNet.TcpPacket tcpPacket, PacketDotNet.IPPacket ipPacket, PcapReadState state)
    {
        var pgPacket = new PostgresPacket();

        // IPV6
        pgPacket.SourceAddress = ipPacket.SourceAddress;
        pgPacket.DestinationAddress = ipPacket.DestinationAddress;

        // TCP
        pgPacket.SourcePort = tcpPacket.SourcePort;
        pgPacket.DestinationPort = tcpPacket.DestinationPort;
        pgPacket.IsFrontEnd = tcpPacket.DestinationPort == state.Port;

        // Reconstruct packet
        byte[] payloadData = state.PreviousBufferLeftover is null ? tcpPacket.PayloadData 
                                                                : [.. state.PreviousBufferLeftover, .. tcpPacket.PayloadData];

        using var memStream = new MemoryStream(payloadData);
        using var reader = new PcapBinaryReader(memStream, Encoding.UTF8);

        byte[]? nextRemainder = null;
        while (reader.BaseStream.Position != memStream.Length)
        {
            var currentPosition = reader.BaseStream.Position;
            ushort clientPort = pgPacket.IsFrontEnd ? pgPacket.SourcePort : pgPacket.DestinationPort;
            if (TryReadMessage(reader, state, pgPacket.IsFrontEnd, (int)clientPort, out var message))
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

        state.PreviousBufferLeftover = nextRemainder;

        return pgPacket;
    }

    private static bool TryReadMessage(PcapBinaryReader reader, PcapReadState state, bool isFrontEnd, int clientPort, out PostgresMessageBase? message)
    {
        try
        {
            message = null;
            var messageCode = reader.ReadChar();

            // Check if auth
            int messageLength = 0;
            if (messageCode == 0)
            {
                messageCode = '?';
                reader.BaseStream.Seek(-1, SeekOrigin.Current);
                messageLength = reader.ReadInt32();
            }

            var pgMessage = PostgresMessages.GetMessage(messageCode, isFrontEnd) ?? PostgresMessages.GetMessage(messageCode, !isFrontEnd);

            if (pgMessage == null)
            {
                throw new NotSupportedException($"Message not supported for code: '{messageCode}'");
            }

            // Check if buffer empty
            if (!reader.HasSufficientData(sizeof(int)) && !state.SSLRequestedFromClient(clientPort)) // code + length
                return false;

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
                nameof(DataRow) => DataRowMessage.Read(messageCode, reader, state.LastRowDescription),
                nameof(CommandComplete) => CommandCompleteMessage.Read(messageCode, reader),
                nameof(NoticeResponse) when state.SSLRequestedFromClient(clientPort) => SSLResponseMessage.Read(messageCode),
                nameof(NoticeResponse) => NoticeResponseMessage.Read(messageCode, reader),
                "ParamData" => SendOutTupleMessage.Read(messageCode, reader, state.LastOutDescription),
                nameof(Terminate) => TerminateMessage.Read(messageCode, reader),
                "StartupMessage" when messageLength == 8 => SSLRequestMessage.Read(messageCode, messageLength, reader),
                "StartupMessage" when messageLength > 8 => StartupMessageMessage.Read(messageCode, messageLength, reader),
                "AuthenticationRequest" => AuthenticationMessage.Read(messageCode, reader),
                "Password" => state.PopLastAuthPacket(clientPort)!.ReadResponseMessage(messageCode, reader),
                nameof(ParameterStatus) => ParameterStatusMessage.Read(messageCode, reader),
                nameof(BackendKeyData) => BackendKeyDataMessage.Read(messageCode, reader),
                nameof(ErrorResponse) => ErrorResponseMessage.Read(messageCode, reader),
                _ => throw new NotImplementedException($"Missing implementation for message '{pgMessage.Value.Name}' (code: '{messageCode}') read."),
            };

            if (message == null) // Cannot read, unsufficient buffer data available
                return false;

            if (message is AuthenticationGenericMessage authMsg)
                state.SetLastAuthPacket(clientPort, authMsg);
            if (message is RowDescriptionMessage rowDesc)
                state.LastRowDescription = rowDesc;
            if (message is OutDescriptionMessage outDesc)
                state.LastOutDescription = outDesc;

            state.SetSSLRequestedFromClient(clientPort, message is SSLRequestMessage);

            return true;
        }
        catch (EndOfStreamException)
        {
            message = null;
            return false;
        }
    }
}
