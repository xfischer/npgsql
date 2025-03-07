using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Diagnostics;
using System.Text;

namespace pcap2latex;

public class PcapService(ILogger<PcapService> logger, IOptions<PcapPostgresOptions> pcapPostgresOptions)
{
    private PcapPostgresOptions options { get; init; } = pcapPostgresOptions.Value;

    public IEnumerable<PostgresPacket> ConvertPcap(string pcapFile, ushort pgsqlPortNumber = 5432)
    {
        PcapReadState state = new()
        {
            Port = pgsqlPortNumber
        };

        using var device = new CaptureFileReaderDevice(pcapFile);
        device.Open(new());

        // Capture packets using GetNextPacket()
        int packetRelativeIndex = 0;
        while (device.GetNextPacket(out PacketCapture e) == GetPacketStatus.PacketRead)
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
                    logger.LogInformation("{Hour}:{Min}:{Sec},{Ms} Len={Len} {SrcIp}:{SrcPort} -> {DstIp}:{DstPort}",
                        time.Hour, time.Minute, time.Second, time.Millisecond, len,
                        srcIp, srcPort, dstIp, dstPort);

                    var pgPacket = ParsePacket(tcpPacket, ipPacket, state);
                    pgPacket.PacketIndex = packetRelativeIndex++;
                    yield return pgPacket;

                }
            }
        }
    }

    private PostgresPacket ParsePacket(PacketDotNet.TcpPacket tcpPacket, PacketDotNet.IPPacket ipPacket, PcapReadState state)
    {
        var pgPacket = new PostgresPacket
        {
            // IPV6
            SourceAddress = ipPacket.SourceAddress,
            DestinationAddress = ipPacket.DestinationAddress,

            // TCP
            SourcePort = tcpPacket.SourcePort,
            DestinationPort = tcpPacket.DestinationPort,
            IsFrontEnd = tcpPacket.DestinationPort == state.Port
        };

        // Reconstruct packet
        byte[] payloadData = state.PreviousBufferLeftover is null ? tcpPacket.PayloadData 
                                                                : [.. state.PreviousBufferLeftover, .. tcpPacket.PayloadData];

        using var memStream = new MemoryStream(payloadData);
        using var binaryReader = new BinaryReader(memStream, Encoding.UTF8);
        using var reader = new PcapBinaryReader(binaryReader, Encoding.UTF8);

        byte[]? nextRemainder = null;
        while (binaryReader.BaseStream.Position != memStream.Length)
        {
            var currentPosition = binaryReader.BaseStream.Position;
            ushort clientPort = pgPacket.IsFrontEnd ? pgPacket.SourcePort : pgPacket.DestinationPort;
            ParserInfo info = new ParserInfo(reader, state, pgPacket.IsFrontEnd, clientPort);
            if (TryReadMessage(info, out var message))
            {
                Debug.Assert(message != null);
                pgPacket.Messages.Add(message!);
            }
            else
            {
                binaryReader.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
                nextRemainder = reader.ReadBytes((int)(memStream.Length - currentPosition));
            }
        }

        state.PreviousBufferLeftover = nextRemainder;

        return pgPacket;
    }

    

    private bool TryReadMessage(ParserInfo info, out IPostgresMessage? message)
    {
        try
        {
            message = null;
            var messageCode = info.Reader.ReadChar();

            // Check if auth
            int messageLength = 0;
            if (messageCode == 0)
            {
                messageCode = '?';
                info.Reader.Seek(-1, SeekOrigin.Current);
                messageLength = info.Reader.ReadInt32();
            }

            var pgMessage = options.MessageCatalog.GetMessage(messageCode, info.IsFrontEnd) ?? options.MessageCatalog.GetMessage(messageCode, !info.IsFrontEnd);

            if (pgMessage == null)
            {
                throw new NotSupportedException($"Message not supported for code: '{messageCode}'");
            }

            // Check if buffer empty
            if (!info.Reader.HasSufficientData(sizeof(int)) && !info.State.SSLRequestedFromClient(info.ClientPort)) // code + length
                return false;

            message = pgMessage.Value.Name! switch
            {
                nameof(Parse) => ParseMessage.Read(messageCode, info.Reader),
                nameof(Bind) => BindMessage.Read(messageCode, info.Reader),
                nameof(Describe) => DescribeMessage.Read(messageCode, info.Reader),
                nameof(Execute) => ExecuteMessage.Read(messageCode, info.Reader),
                nameof(Sync) => SyncMessage.Read(messageCode, info.Reader),
                nameof(Query) => QueryMessage.Read(messageCode, info.Reader),
                nameof(NoData) => NoDataMessage.Read(messageCode, info.Reader),
                nameof(BindComplete) => BindCompleteMessage.Read(messageCode, info.Reader),
                nameof(ParseComplete) => ParseCompleteMessage.Read(messageCode, info.Reader),
                nameof(ParameterDescription) => ParameterDescriptionMessage.Read(messageCode, info.Reader),
                nameof(RowDescription) => RowDescriptionMessage.Read(messageCode, info.Reader),
                nameof(ReadyForQuery) => ReadyForQueryMessage.Read(messageCode, info.Reader),
                nameof(DataRow) => DataRowMessage.Read(messageCode, info.Reader, info.State.LastRowDescription),
                nameof(CommandComplete) => CommandCompleteMessage.Read(messageCode, info.Reader),
                nameof(NoticeResponse) when info.State.SSLRequestedFromClient(info.ClientPort) => SSLResponseMessage.Read(messageCode),
                nameof(NoticeResponse) => NoticeResponseMessage.Read(messageCode, info.Reader),
                nameof(Terminate) => TerminateMessage.Read(messageCode, info.Reader),
                "StartupMessage" when messageLength == 8 => SSLRequestMessage.Read(messageCode, messageLength, info.Reader),
                "StartupMessage" when messageLength > 8 => StartupMessageMessage.Read(messageCode, messageLength, info.Reader),
                "AuthenticationRequest" => AuthenticationMessage.Read(messageCode, info.Reader),
                "Password" => info.State.GetLastAuthPacket(info.ClientPort)!.ReadResponseMessage(messageCode, info.Reader),
                nameof(ParameterStatus) => ParameterStatusMessage.Read(messageCode, info.Reader),
                nameof(BackendKeyData) => BackendKeyDataMessage.Read(messageCode, info.Reader),
                nameof(ErrorResponse) => ErrorResponseMessage.Read(messageCode, info.Reader),
                _ => options.CustomMessageProcessor?.Invoke(pgMessage.Value, info) ?? throw new NotImplementedException($"Missing implementation for message '{pgMessage.Value.Name}' (code: '{messageCode}') read."),
            };

            if (message == null) // Cannot read, unsufficient buffer data available
                return false;

            if (message is AuthenticationGenericMessage authMsg)
                info.State.SetLastAuthPacket(info.ClientPort, authMsg);
            if (message is RowDescriptionMessage rowDesc)
                info.State.LastRowDescription = rowDesc;

            info.State.SetSSLRequestedFromClient(info.ClientPort, message is SSLRequestMessage);

            return true;
        }
        catch (EndOfStreamException)
        {
            message = null;
            return false;
        }
    }
}
