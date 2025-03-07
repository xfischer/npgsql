using System.Net;

namespace pcap2latex;

public class PostgresPacket
{
    public List<IPostgresMessage> Messages { get; set; } = [];
    public bool IsFrontEnd { get; internal set; }
    public IPAddress SourceAddress { get; internal set; } = IPAddress.Loopback;
    public IPAddress DestinationAddress { get; internal set; } = IPAddress.Loopback;
    public ushort SourcePort { get; internal set; }
    public ushort DestinationPort { get; internal set; }
    public int PacketIndex { get; internal set; }
}
