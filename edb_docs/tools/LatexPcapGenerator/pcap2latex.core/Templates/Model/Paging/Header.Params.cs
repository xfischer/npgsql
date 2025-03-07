namespace pcap2latex;

public partial class Header(string? message, GenerationState state) : ITextTransformer
{
    public string Message { get; } = LatexHelper.Unescape(message ?? string.Empty);
    public GenerationState State { get; } = state;
}
