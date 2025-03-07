using static pcap2latex.LatexHelper;

namespace pcap2latex;

public partial class CommandComplete(CommandCompleteMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public string Tag { get; } = Unescape(message.Message);
}
