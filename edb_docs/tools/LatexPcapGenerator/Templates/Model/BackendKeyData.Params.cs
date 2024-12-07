namespace pcap2latex.Templates
{
    public partial class BackendKeyData : ITextTransformer
    {
        public int Length { get; }
        public int ProcessID { get; }
        public uint SecretKey { get; }

        public BackendKeyData(BackendKeyDataMessage message)
        {
            Length = message.Length;
            ProcessID = message.ProcessId;
            SecretKey = message.SecretKey;
        }
    }
}
