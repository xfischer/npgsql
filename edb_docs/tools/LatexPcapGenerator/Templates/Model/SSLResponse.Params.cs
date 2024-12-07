namespace pcap2latex.Templates
{
    public partial class SSLResponse : ITextTransformer
    {
        public string Type { get; }
        public string Response { get; }

        public SSLResponse(SSLResponseMessage message)
        {
            Type = nameof(SSLResponse);
            Response = message.Code.ToString();
        }
    }
}
