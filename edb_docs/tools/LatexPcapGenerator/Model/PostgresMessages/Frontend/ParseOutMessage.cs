namespace pcap2latex;

public class ParseOutMessage(char code, int length) : PostgresMessageBase(code, length)
{    
    public string Statement { get; internal set; } = "";
    public string Query { get; internal set; } = "";
    public short ParameterCount { get; internal set; }
    public List<int> ParameterOids { get; internal set; } = new();
    public List<short> ParameterDirections { get; internal set; } = new();

    internal static ParseOutMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new ParseOutMessage(messageCode, len);
        packet.Statement = reader.ReadNullTerminatedString(len);
        packet.Query = reader.ReadNullTerminatedString(len);
        packet.ParameterCount = reader.ReadInt16();

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
