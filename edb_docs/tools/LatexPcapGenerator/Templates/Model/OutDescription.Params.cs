namespace pcap2latex.Templates;

public partial class OutDescription : ITextTransformer
{
    public int Length { get; }
    public int FieldCount { get; }
    public List<OutParamDescription> Columns { get; }
    
    public OutDescription(OutDescriptionMessage message)
    {
        Length = message.Length;
        FieldCount = message.FieldCount;
        Columns = message.ParameterDescriptions;
    }
}
