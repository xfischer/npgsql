using pcap2latex;

namespace epascap2latex;

public class ParseOutMessage(char code, int length) : PostgresMessageBase(code, length)
{    
    public string Statement { get; internal set; } = "";
    public string Query { get; internal set; } = "";
    public short ParameterCount { get; internal set; }
    public List<int> ParameterOids { get; internal set; } = [];
    public List<short> ParameterDirections { get; internal set; } = [];

    public static ParseOutMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new ParseOutMessage(messageCode, len)
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
