namespace pcap2latex.Templates;

public partial class Query : ITextTransformer
{
    public char MessageCode { get; }

    public int Length { get; }

    public string QueryText { get; }

    public Query(QueryMessage message)
    {
        MessageCode = message.Code;
        Length = message.Length;
        QueryText = LatexHelper.TrimUnescape(message.Query, maxLength: 50);
    }
}
