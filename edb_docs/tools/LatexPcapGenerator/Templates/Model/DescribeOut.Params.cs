using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class DescribeOut : ITextTransformer
{
    public int Length { get; }
    public string StatementOrPortalCode { get; }
    public string StatementOrPortalName { get; }
    public string StatementOrPortalCaption { get; }

    public DescribeOut(DescribeOutMessage message) {
        Length = message.Length;
        StatementOrPortalCode = message.PortalOrStatement.ToString();
        StatementOrPortalName = Unescape(message.PortalOrStatementName);
        StatementOrPortalCaption = message.PortalOrStatement == 'S' ? "statement name: " : "portal name: ";
        StatementOrPortalCaption += StatementOrPortalName;
    }
}
