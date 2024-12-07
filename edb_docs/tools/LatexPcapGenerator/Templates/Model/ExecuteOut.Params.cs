namespace pcap2latex.Templates;

public partial class ExecuteOut : ITextTransformer
{
    public int Length { get; }

    public ExecuteOut(ExecuteOutMessage message) {
        Length = message.Length;
    }
}
