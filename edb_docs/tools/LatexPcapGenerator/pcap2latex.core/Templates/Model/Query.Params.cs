namespace pcap2latex.Templates;

public partial class Query(QueryMessage message) : ITextTransformer
{
    public char MessageCode { get; } = message.Code;

    public int Length { get; } = message.Length;

    public string QueryText { get; } = LatexHelper.TrimUnescape(message.Query, maxLength: 50);
}
