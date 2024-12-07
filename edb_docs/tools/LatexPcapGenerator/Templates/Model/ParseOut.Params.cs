using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class ParseOut : ITextTransformer
{
    public int Length { get; }
    public string Statement { get; }
    public int StatementLength { get; }
    public string Query { get; }
    public int QueryLength { get; }
    public List<int> ParameterTypes { get; }
    public List<short> ParameterDirections { get; }

    public ParseOut(ParseOutMessage message) {

        Length = message.Length;
        Statement = Unescape(message.Statement);
        StatementLength = message.Length;
        Query = Unescape(message.Query);
        QueryLength = message.Query.Length;
        ParameterTypes = message.ParameterOids;
        ParameterDirections = message.ParameterDirections;
    }

    
}
