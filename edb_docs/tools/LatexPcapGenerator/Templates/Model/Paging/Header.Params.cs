namespace pcap2latex.Templates.Paging;

public partial class Header
{
    public string Message { get; }
    public GenerationState State { get; }

    public Header(string? message, GenerationState state)
    {
        Message = LatexHelper.Unescape(message ?? string.Empty);
        State = state;
    }
}
