using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class Execute : ITextTransformer
{
    public int Length { get; }
    public string Portal { get; }
    public int MaxRows { get; }

    public Execute(ExecuteMessage message) {
        Length = message.Length;
        Portal = Unescape(message.PortalName);
        MaxRows = message.MaxRows;
    }
}
