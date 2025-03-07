namespace pcap2latex;

public partial class SkippedWords(string messageName, int skippedItems) : ITextTransformer
{
    public string MessageName { get; } = messageName;
    public int SkippedItems { get; } = skippedItems;
}
