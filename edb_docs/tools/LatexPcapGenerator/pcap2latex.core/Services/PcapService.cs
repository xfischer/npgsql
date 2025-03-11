using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Diagnostics;
using System.Text;

namespace pcap2latex;

public class PcapService(ILogger<PcapService> logger, IOptions<PcapPostgresOptions> pcapPostgresOptions)
{
    public static PcapService Create(ILoggerFactory? loggerFactory = null, PcapPostgresOptions? options = null)
    {
        if (options is null)
        {
            options = new PcapPostgresOptions();
            options.AddDefaultPostgresMessages();
        }

        var logger = loggerFactory == null ?
                        NullLogger<PcapService>.Instance
                        : loggerFactory.CreateLogger<PcapService>();
        return new PcapService(logger, Options.Create(options));
    }

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
                    pgPacket.Timestamp = time;
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

    private bool TryReadMessage(ParserInfo info, out PostgresMessageBase? message)
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

            var pgMessageRaw = options.MessageCatalog.GetMessage(messageCode, info.IsFrontEnd) ?? options.MessageCatalog.GetMessage(messageCode, !info.IsFrontEnd);

            if (pgMessageRaw == null)
            {
                throw new NotSupportedException($"Message not supported for code: '{messageCode}'");
            }
            var pgMessage = pgMessageRaw.Value with { IsFrontEnd = info.IsFrontEnd };

            // Check if buffer empty
            if (!info.Reader.HasSufficientData(sizeof(int)) && !info.State.SSLRequestedFromClient(info.ClientPort)) // code + length
                return false;

            message = pgMessage.Name switch
            {
                nameof(Parse) => ParseMessage.Read(pgMessage, info.Reader),
                nameof(Bind) => BindMessage.Read(pgMessage, info.Reader),
                nameof(Describe) => DescribeMessage.Read(pgMessage, info.Reader),
                nameof(Execute) => ExecuteMessage.Read(pgMessage, info.Reader),
                nameof(Sync) => SyncMessage.Read(pgMessage, info.Reader),
                nameof(Query) => QueryMessage.Read(pgMessage, info.Reader),
                nameof(NoData) => NoDataMessage.Read(pgMessage, info.Reader),
                nameof(BindComplete) => BindCompleteMessage.Read(pgMessage, info.Reader),
                nameof(ParseComplete) => ParseCompleteMessage.Read(pgMessage, info.Reader),
                nameof(ParameterDescription) => ParameterDescriptionMessage.Read(pgMessage, info.Reader),
                nameof(RowDescription) => RowDescriptionMessage.Read(pgMessage, info.Reader),
                nameof(ReadyForQuery) => ReadyForQueryMessage.Read(pgMessage, info.Reader),
                nameof(DataRow) => DataRowMessage.Read(pgMessage, info.Reader, info.State.LastRowDescription),
                nameof(CommandComplete) => CommandCompleteMessage.Read(pgMessage, info.Reader),
                nameof(NoticeResponse) when info.State.SSLRequestedFromClient(info.ClientPort) => SSLResponseMessage.Read(pgMessage),
                nameof(NoticeResponse) => NoticeResponseMessage.Read(pgMessage, info.Reader),
                nameof(Terminate) => TerminateMessage.Read(pgMessage, info.Reader),
                "StartupMessage" when messageLength == 8 => SSLRequestMessage.Read(pgMessage, messageLength, info.Reader),
                "StartupMessage" when messageLength > 8 => StartupMessageMessage.Read(pgMessage, messageLength, info.Reader),
                "AuthenticationRequest" => AuthenticationMessage.Read(pgMessage, info.Reader),
                "Password" => info.State.GetLastAuthPacket(info.ClientPort)!.ReadResponseMessage(pgMessage, info.Reader),
                nameof(ParameterStatus) => ParameterStatusMessage.Read(pgMessage, info.Reader),
                nameof(BackendKeyData) => BackendKeyDataMessage.Read(pgMessage, info.Reader),
                nameof(ErrorResponse) => ErrorResponseMessage.Read(pgMessage, info.Reader),
                _ => options.CustomMessageProcessor?.Invoke(pgMessage, info) ?? throw new NotImplementedException($"Missing implementation for message '{pgMessage.Name}' (code: '{messageCode}') read."),
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
