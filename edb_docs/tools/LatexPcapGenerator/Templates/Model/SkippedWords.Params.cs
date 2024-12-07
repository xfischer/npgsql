namespace pcap2latex.Templates;

public partial class SkippedWords : ITextTransformer
{
    public string MessageName { get; }
    public int SkippedItems { get; }

    public SkippedWords(string messageName, int skippedItems) {
        MessageName = messageName;
        SkippedItems = skippedItems;
    }   
}
