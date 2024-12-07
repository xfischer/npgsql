using System.Net;

namespace pcap2latex.Model
{
    public class PostgresPacket
    {
        public List<PostgresMessageBase> Messages { get; set; } = new();
        public bool IsFrontEnd { get; internal set; }
        public IPAddress SourceAddress { get; internal set; } = IPAddress.Loopback;
        public IPAddress DestinationAddress { get; internal set; } = IPAddress.Loopback;
        public ushort SourcePort { get; internal set; }
        public ushort DestinationPort { get; internal set; }
        public int PacketIndex { get; internal set; }
    }
}
