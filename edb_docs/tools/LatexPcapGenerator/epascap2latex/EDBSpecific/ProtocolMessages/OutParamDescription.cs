using pcap2latex;

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
        OutParamDescription description = new()
        {
            ColumnName = reader.ReadNullTerminatedString(maxLength),
            ColumnIndex = reader.ReadInt16(),
            TypeOid = reader.ReadInt32(),
            ColumnLength = reader.ReadInt16(),
            TypeModifier = reader.ReadInt32(),
            Format = reader.ReadInt16()
        };

        return description;
    }
}
