using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class DataRow : ITextTransformer
{
    public int Length { get; }
    public int FieldCount { get; }

    public List<(int Length, string? Data)> Fields { get; } = new();

    public DataRow(DataRowMessage message)
    {
        Length = message.Length;
        FieldCount = message.FieldCount;
        foreach (var f in message.ColumnValues)
        {
            if (f.Length > 0)
            {
                Fields.Add((f.Length, TrimUnescape(Convert.ToHexStringLower(f.Data!), 50)));
            }
            else
            {
                Fields.Add((f.Length, string.Empty));
            }

        }
    }


}
