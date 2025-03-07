namespace pcap2latex;

public partial class BackendKeyData(BackendKeyDataMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public int ProcessID { get; } = message.ProcessId;
    public uint SecretKey { get; } = message.SecretKey;
}
