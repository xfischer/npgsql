namespace pcap2latex;

public class FieldDescription
{
    public string ColumnName { get; private set; } = "";
    public int TableOid { get; private set; }
    public short ColumnIndex { get; private set; }
    public int TypeOid { get; private set; }
    public short ColumnLength { get; private set; }
    public int TypeModifier { get; private set; }
    public short Format { get; private set; }

    internal static FieldDescription Read(PcapBinaryReader reader, int maxLength)
    {
        FieldDescription description = new()
        {
            ColumnName = reader.ReadNullTerminatedString(maxLength),
            TableOid = reader.ReadInt32(),
            ColumnIndex = reader.ReadInt16(),
            TypeOid = reader.ReadInt32(),
            ColumnLength = reader.ReadInt16(),
            TypeModifier = reader.ReadInt32(),
            Format = reader.ReadInt16()
        };

        return description;
    }
}

