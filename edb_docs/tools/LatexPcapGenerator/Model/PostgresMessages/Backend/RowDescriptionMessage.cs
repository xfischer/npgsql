namespace pcap2latex;

public class RowDescriptionMessage(char code, int length) : PostgresMessageBase(code, length)
{

    public short FieldCount { get; internal set; }

    public List<FieldDescription> FieldDescriptions { get; internal set; } = new();

    internal static RowDescriptionMessage? Read(char messageCode, PcapBinaryReader reader)
    {
        if (!reader.HasSufficientData(4))
            return null;
        var len = reader.ReadInt32();

        if (!reader.HasSufficientData(len))
            return null;

        var message = new RowDescriptionMessage(messageCode, len);
        message.FieldCount = reader.ReadInt16();
        
        for (int i = 0; i < message.FieldCount; i++)
        {
            message.FieldDescriptions.Add(FieldDescription.Read(reader, len));
        }

        return message;
    }
}
