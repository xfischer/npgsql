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
        FieldDescription description = new();
        description.ColumnName = reader.ReadNullTerminatedString(maxLength);
        description.TableOid = reader.ReadInt32();
        description.ColumnIndex = reader.ReadInt16();
        description.TypeOid= reader.ReadInt32();
        description.ColumnLength = reader.ReadInt16();
        description.TypeModifier = reader.ReadInt32();
        description.Format = reader.ReadInt16();

        return description;
    }

    internal static FieldDescription Read(Serialization.Field column)
    {
        FieldDescription description = new();
        description.ColumnName = column.Show;
        description.TableOid = int.Parse(column.Fields[0].Show);
        description.ColumnIndex = short.Parse(column.Fields[1].Show);
        description.TypeOid = int.Parse(column.Fields[2].Show);
        description.ColumnLength = short.Parse(column.Fields[3].Show);
        description.TypeModifier = int.Parse(column.Fields[4].Show);
        description.Format = short.Parse(column.Fields[5].Show);

        return description;
    }
}

