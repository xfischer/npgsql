using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class SendOutTuple : ITextTransformer
{
    public int Length { get; }
    public int FieldCount { get; }
    public List<(int Length, string? Data)> Parameters { get; } = new();

    public SendOutTuple(SendOutTupleMessage message) {

        Length = message.Length;
        FieldCount = message.ParameterCount;
        foreach (var f in message.ParameterValues)
        {
            if (f.Length >0)
            {
                Parameters.Add((f.Length, TrimUnescape(Convert.ToHexStringLower(f.Data!), 65)));
            }
            else
            {
                Parameters.Add((f.Length, string.Empty));
            }

        }
    }

    
}
