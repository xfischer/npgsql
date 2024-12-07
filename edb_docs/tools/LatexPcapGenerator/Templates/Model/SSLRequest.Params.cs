namespace pcap2latex.Templates
{
    public partial class SSLRequest : ITextTransformer
    {
        public string Type { get; }
        public int Length { get; }
        public int Payload { get; }

        public SSLRequest(SSLRequestMessage message)
        {
            Length = message.Length;
            Type  =nameof(SSLRequest);
            Payload = message.Payload;
        }
    }
}
