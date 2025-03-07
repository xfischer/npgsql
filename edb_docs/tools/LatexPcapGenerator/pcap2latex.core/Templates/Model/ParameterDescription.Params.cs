namespace pcap2latex;

public partial class ParameterDescription(ParameterDescriptionMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public int ParamCount { get; } = message.ParameterCount;
    public List<int> ParameterTypes { get; } = message.ParameterOids;
}
