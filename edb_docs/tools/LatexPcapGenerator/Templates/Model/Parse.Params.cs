using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class Parse : ITextTransformer
{
    public int Length { get; }
    public string Statement { get; }
    public int StatementLength { get; }
    public string Query { get; }
    public int QueryLength { get; }
    public List<int> ParameterOids { get; }

    public Parse(ParseMessage message) {

        Length = message.Length;
        Statement = Unescape(message.Statement);
        StatementLength = message.Statement.Length;
        Query = Unescape(message.Query);
        QueryLength = message.Query.Length;
        ParameterOids = message.ParameterOids;
    }

    
}
