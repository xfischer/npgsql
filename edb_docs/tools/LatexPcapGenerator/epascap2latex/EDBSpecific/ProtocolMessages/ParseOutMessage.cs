using pcap2latex;

namespace epascap2latex;

public class ParseOutMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{    
    public string Statement { get; internal set; } = "";
    public string Query { get; internal set; } = "";
    public short ParameterCount { get; internal set; }
    public List<int> ParameterOids { get; internal set; } = [];
    public List<short> ParameterDirections { get; internal set; } = [];

    public static ParseOutMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new ParseOutMessage(pgMessage, len)
        {
            Statement = reader.ReadNullTerminatedString(len),
            Query = reader.ReadNullTerminatedString(len),
            ParameterCount = reader.ReadInt16()
        };

        for (int i = 0; i < packet.ParameterCount; i++)
        {
            packet.ParameterOids.Add(reader.ReadInt32());
        }
        for (int i = 0; i < packet.ParameterCount; i++)
        {
            packet.ParameterDirections.Add(reader.ReadInt16());
        }
        return packet;
    }
}
