namespace pcap2latex;

public class RowDescriptionMessage(char code, int length) : PostgresMessageBase(code, length)
{

    public short FieldCount { get; internal set; }

    public List<FieldDescription> FieldDescriptions { get; internal set; } = new();

    internal static RowDescriptionMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new RowDescriptionMessage(messageCode, len);
        message.FieldCount = reader.ReadInt16();
        
        for (int i = 0; i < message.FieldCount; i++)
        {
            message.FieldDescriptions.Add(FieldDescription.Read(reader, len));
        }

        return message;
    }

    internal static RowDescriptionMessage Read(char messageCode, Serialization.Proto proto)
    {
        var len = Convert.ToInt16(proto.Fields[1].Value, 16);
        var message = new RowDescriptionMessage(messageCode, len);
        message.FieldCount = Convert.ToInt16(proto.Fields[3].Value, 16);

        foreach(var field in proto.Fields[3].Fields) 
        {
            message.FieldDescriptions.Add(FieldDescription.Read(field));
        }

        return message;
    }
}
