using pcap2latex;

namespace epascap2latex;

public partial class OutDescription(OutDescriptionMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public int FieldCount { get; } = message.FieldCount;
    public List<OutParamDescription> Columns { get; } = message.ParameterDescriptions;
}
