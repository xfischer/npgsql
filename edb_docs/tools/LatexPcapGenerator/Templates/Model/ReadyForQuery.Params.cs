using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class ReadyForQuery : ITextTransformer
{
    public int Length { get; }
    public string Status { get; }

    public ReadyForQuery(ReadyForQueryMessage message) {

        Length = message.Length;
        Status = Unescape(message.Status.ToString());
    }

    
}
