namespace pcap2latex;

public class RowDescriptionMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{

    public short FieldCount { get; internal set; }

    public List<FieldDescription> FieldDescriptions { get; internal set; } = [];

    internal static RowDescriptionMessage? Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        if (!reader.HasSufficientData(4))
            return null;
        var len = reader.ReadInt32();

        if (!reader.HasSufficientData(len))
            return null;

        var message = new RowDescriptionMessage(pgMessage, len)
        {
            FieldCount = reader.ReadInt16()
        };

        for (int i = 0; i < message.FieldCount; i++)
        {
            message.FieldDescriptions.Add(FieldDescription.Read(reader, len));
        }

        return message;
    }

    public override string GetStringRepresentation() => $"[{string.Join(", ", 
        FieldDescriptions.Select(f => $"{f.ColumnName}: {f.TableOid}, {f.ColumnIndex}, {f.TypeOid}, {f.ColumnLength}, {f.TypeModifier}, {f.Format}"))}]";
}
