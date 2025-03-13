using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class RowDescription(RowDescriptionMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public int FieldCount { get; } = message.FieldCount;
    public List<FieldDescription> Fields { get; } = message.FieldDescriptions;
    public string ColFormat(FieldDescription column) => ToFormatString(column.Format);
    public string ColumnName(FieldDescription column) => Unescape(column.ColumnName);
}
