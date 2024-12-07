using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class SASLInitialResponse : ITextTransformer
{
    public int Length { get; }
    public string Mechanism { get; set; }
    public int InitialResponseLength { get; set; }
    public string InitialResponse { get; set; }

    public SASLInitialResponse(SASLInitialResponseMessage message)
    {
        Length = message.Length;
        Mechanism = Unescape(message.Mechanism);
        InitialResponseLength = message.InitialResponseLength;
        InitialResponse = TrimUnescape("Initial response: " + Convert.ToHexStringLower(message.InitialResponse), 50);
    }
}
