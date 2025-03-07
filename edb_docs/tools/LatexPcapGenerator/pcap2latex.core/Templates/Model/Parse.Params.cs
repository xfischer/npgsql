using static pcap2latex.LatexHelper;

namespace pcap2latex;

public partial class Parse(ParseMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public string Statement { get; } = Unescape(message.Statement);
    public int StatementLength { get; } = message.Statement.Length;
    public string Query { get; } = Unescape(message.Query);
    public int QueryLength { get; } = message.Query.Length;
    public List<int> ParameterOids { get; } = message.ParameterOids;
}
