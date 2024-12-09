using System;
using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class DataRow : ITextTransformer
{
    public int Length { get; }
    public int FieldCount { get; }

    public List<(int Length, string Name, string? Data)> Fields { get; } = new();

    public DataRow(DataRowMessage message)
    {
        Length = message.Length;
        FieldCount = message.FieldCount;
        int index = 0;
        foreach (var f in message.ColumnValues)
        {
            var fieldLabel = string.IsNullOrEmpty(f.Name) ? $"field {index}" : $"field {index}  \\\\ {TrimUnescape(f.Name, 25)}";
            if (f.Length > 0)
            {
                if (f.IsText && f.TextRepresentation != null)
                {
                    Fields.Add((f.Length, fieldLabel, TrimUnescape(f.TextRepresentation, 50)));
                }
                else
                {
                    Fields.Add((f.Length, fieldLabel, TrimUnescape(Convert.ToHexStringLower(f.Data!), 50)));
                }
            }
            else
            {
                Fields.Add((f.Length, fieldLabel, string.Empty));
            }
            index++;
        }
    }


}
