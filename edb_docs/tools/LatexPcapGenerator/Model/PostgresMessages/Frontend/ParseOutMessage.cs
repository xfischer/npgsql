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
    internal static ParseOutMessage Read(char messageCode, Serialization.Proto proto)
    {
        var len = Convert.ToInt16(proto.Fields[1].Value, 16);
        var message = new ParseOutMessage(messageCode, len);
        message.Statement = proto.Fields[3].Show;
        message.Query = proto.Fields[4].Show;
        message.ParameterCount = Convert.ToInt16(proto.Fields[5].Value, 16);
        message.ParameterOids = proto.Fields[5].Fields.Select(f => int.Parse(f.Show)).ToList();
        message.ParameterDirections = proto.Fields[6].Fields.Select(f => short.Parse(f.Show)).ToList();
        return message;
    }


}
