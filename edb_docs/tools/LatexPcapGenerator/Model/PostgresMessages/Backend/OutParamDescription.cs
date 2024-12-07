namespace pcap2latex;

public class OutParamDescription
{
    public string ColumnName { get; internal set; } = "";
    public short ColumnIndex { get; internal set; }
    public int TypeOid { get; internal set; }
    public short ColumnLength { get; internal set; }
    public int TypeModifier { get; internal set; }
    public short Format { get; internal set; }

    internal static OutParamDescription Read(PcapBinaryReader reader, int maxLength)
    {
        OutParamDescription description = new();
        description.ColumnName = reader.ReadNullTerminatedString(maxLength);
        description.ColumnIndex = reader.ReadInt16();
        description.TypeOid= reader.ReadInt32();
        description.ColumnLength = reader.ReadInt16();
        description.TypeModifier = reader.ReadInt32();
        description.Format = reader.ReadInt16();

        return description;
    }
}
