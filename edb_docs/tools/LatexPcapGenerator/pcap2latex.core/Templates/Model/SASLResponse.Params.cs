using static pcap2latex.LatexHelper;

namespace pcap2latex;

public partial class SASLResponse(SASLResponseMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public string Data { get; set; } = TrimUnescape("AuthData: " + Convert.ToHexStringLower(message.AuthData), 50);
}
