using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class SASLResponse : ITextTransformer
{
    public int Length { get; }
    public string Data { get; set; }
    public SASLResponse(SASLResponseMessage message)
    {
        Length = message.Length;
        Data = TrimUnescape("AuthData: " + Convert.ToHexStringLower(message.AuthData), 50);
    }
}
