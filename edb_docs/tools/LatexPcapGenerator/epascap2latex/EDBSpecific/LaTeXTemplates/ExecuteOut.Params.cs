using pcap2latex;

namespace epascap2latex;

public partial class ExecuteOut(ExecuteOutMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
}
