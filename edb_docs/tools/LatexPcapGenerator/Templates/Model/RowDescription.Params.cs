using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class RowDescription : ITextTransformer
{
    public int Length { get; }
    public int FieldCount { get; }
    public List<FieldDescription> Fields { get; }
    public string ColFormat(FieldDescription column) => ToFormatString(column.Format);
    public string ColumnName(FieldDescription column) => Unescape(column.ColumnName);


    public RowDescription(RowDescriptionMessage message) {
        Length = message.Length;
        FieldCount = message.FieldCount;
        Fields = message.FieldDescriptions;
    }
}
