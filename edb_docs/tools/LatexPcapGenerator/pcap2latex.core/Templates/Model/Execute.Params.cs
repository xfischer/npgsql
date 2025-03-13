using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class Execute(ExecuteMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public string Portal { get; } = Unescape(message.PortalName);
    public int MaxRows { get; } = message.MaxRows;
}
