namespace pcap2latex.Templates;

public partial class Bind : ITextTransformer
{
    public int Length { get; }
    public string Statement { get; }
    public int ParamFormatCount { get; }
    public List<short> ParamFormats { get; }
    public short ParamValuesCount { get; }
    public List<(int Length, byte[] Data)> ParamValues { get; }
    public short ResultFormatsCount { get; }
    public List<short> ResultFormats { get; }
    public string Portal{ get; }


    public Bind(BindMessage message)
    {
        Length = message.Length;
        Portal = message.PortalName;
        Statement = message.StatementName;
        ParamFormatCount = message.ParameterFormatsCount;
        ParamFormats = message.ParameterFormats;
        ParamValuesCount = message.ParameterValuesCount;
        ParamValues = message.ParameterValues;
        ResultFormatsCount = message.ResultsFormatCount;
        ResultFormats = message.ResultsFormat;
    }
}
