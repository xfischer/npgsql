using pcap2latex;
using static pcap2latex.LatexHelper;

namespace epascap2latex;

public partial class ParseOut(ParseOutMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public string Statement { get; } = Unescape(message.Statement);
    public int StatementLength { get; } = message.Length;
    public string Query { get; } = Unescape(message.Query);
    public int QueryLength { get; } = message.Query.Length;
    public List<int> ParameterTypes { get; } = message.ParameterOids;
    public List<short> ParameterDirections { get; } = message.ParameterDirections;
}
