using static pcap2latex.LatexHelper;

namespace pcap2latex;

public partial class ReadyForQuery(ReadyForQueryMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public string Status { get; } = Unescape(message.Status.ToString());
}
