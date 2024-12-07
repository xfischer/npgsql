namespace pcap2latex.Templates
{
    public partial class ParameterStatus : ITextTransformer
    {
        public int Length { get; }
        public string ParamName { get; }
        public string ParamValue { get; }

        public ParameterStatus(ParameterStatusMessage message)
        {
            Length = message.Length;
            ParamName = LatexHelper.TrimUnescape(message.Name, 50);
            ParamValue = LatexHelper.TrimUnescape(message.Value, 50);
        }
    }
}
