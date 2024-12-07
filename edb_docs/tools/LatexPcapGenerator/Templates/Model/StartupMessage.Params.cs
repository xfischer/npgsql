namespace pcap2latex.Templates
{
    public partial class StartupMessage : ITextTransformer
    {
        public string Type { get; }
        public int Length { get; }
        public short VersionMajor { get; }
        public short VersionMinor { get; }
        public List<(string ParamName, string ParamValue)> Parameters { get; }

        public StartupMessage(StartupMessageMessage message)
        {
            Type = LatexHelper.Unescape(nameof(StartupMessage));
            Length = message.Length;
            VersionMajor = message.ProtocolMajorVersion;
            VersionMinor = message.ProtocolMinorVersion;

            Parameters = new();
            foreach (var kvp in message.Parameters)
            {
                Parameters.Add((LatexHelper.TrimUnescape(kvp.Key, 50), LatexHelper.TrimUnescape(kvp.Value, 50)));
            }
        }
    }
}
