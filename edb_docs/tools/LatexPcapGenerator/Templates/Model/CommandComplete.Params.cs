using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class CommandComplete : ITextTransformer
{
    public int Length { get; }
    public string Tag { get; }

    public CommandComplete(CommandCompleteMessage message) {

        Length = message.Length;
        Tag = Unescape(message.Message);
    }

    
}
