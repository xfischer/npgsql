namespace pcap2latex;

public class ParameterDescriptionMessage(char code, int length) : PostgresMessageBase(code, length)
{

    public short ParameterCount { get; internal set; }

    public List<int> ParameterOids { get; internal set; } = new();

    internal static ParameterDescriptionMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ParameterDescriptionMessage(messageCode, len);
        message.ParameterCount = reader.ReadInt16();
        
        for (int i = 0; i < message.ParameterCount; i++)
        {
            message.ParameterOids.Add(reader.ReadInt32());
        }

        return message;
    }

    internal static ParameterDescriptionMessage Read(char messageCode, Serialization.Proto proto)
    {
        var len = Convert.ToInt16(proto.Fields[1].Value, 16);
        var message = new ParameterDescriptionMessage(messageCode, len);
        message.ParameterCount = Convert.ToInt16(proto.Fields[3].Value, 16);
        message.ParameterOids = proto.Fields[3].Fields.Select(f => int.Parse(f.Show)).ToList();

        return message;
    }
}
