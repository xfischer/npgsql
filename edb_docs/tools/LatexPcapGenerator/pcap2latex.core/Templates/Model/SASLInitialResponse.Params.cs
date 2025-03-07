using static pcap2latex.LatexHelper;

namespace pcap2latex;

public partial class SASLInitialResponse(SASLInitialResponseMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public string Mechanism { get; set; } = Unescape(message.Mechanism);
    public int InitialResponseLength { get; set; } = message.InitialResponseLength;
    public string InitialResponse { get; set; } = TrimUnescape("Initial response: " + Convert.ToHexStringLower(message.InitialResponse), 50);
}
