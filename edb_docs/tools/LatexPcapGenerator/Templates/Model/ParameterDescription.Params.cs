namespace pcap2latex.Templates;

public partial class ParameterDescription : ITextTransformer
{
    public int Length { get; }
    public int ParamCount{ get; }
    public List<int> ParameterTypes { get; }

    public ParameterDescription(ParameterDescriptionMessage message) {
        Length = message.Length;
        ParamCount = message.ParameterCount;
        ParameterTypes = message.ParameterOids;
    }
}
