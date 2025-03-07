namespace pcap2latex;

public partial class SSLResponse(SSLResponseMessage message) : ITextTransformer
{
    public string Type { get; } = nameof(SSLResponse);
    public string Response { get; } = message.Code.ToString();
}
