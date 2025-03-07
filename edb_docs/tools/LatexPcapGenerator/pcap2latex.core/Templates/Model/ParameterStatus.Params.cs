namespace pcap2latex;

public partial class ParameterStatus(ParameterStatusMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public string ParamName { get; } = LatexHelper.TrimUnescape(message.Name, 50);
    public string ParamValue { get; } = LatexHelper.TrimUnescape(message.Value, 50);
}
