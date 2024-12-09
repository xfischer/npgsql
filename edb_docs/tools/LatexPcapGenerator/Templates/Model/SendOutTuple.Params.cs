using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class SendOutTuple : ITextTransformer
{
    public int Length { get; }
    public int FieldCount { get; }
    public List<(int Length, string Name, string? Data)> Parameters { get; } = new();

    public SendOutTuple(SendOutTupleMessage message) {

        Length = message.Length;
        FieldCount = message.ParameterCount;
        int index = 0;
        foreach (var f in message.ParameterValues)
        {
            var fieldLabel = string.IsNullOrEmpty(f.Name) ? $"param {index}" : $"param {index} \\\\ {TrimUnescape(f.Name, 25)}";
            if (f.Length >0)
            {
                if (f.IsText && f.TextRepresentation != null)
                {
                    Parameters.Add((f.Length, fieldLabel, TrimUnescape(f.TextRepresentation, 50)));
                }
                else
                {
                    Parameters.Add((f.Length, fieldLabel, TrimUnescape(Convert.ToHexStringLower(f.Data!), 50)));
                }
            }
            else
            {
                Parameters.Add((f.Length, fieldLabel, string.Empty));
            }
            index++;
        }
    }

    
}
