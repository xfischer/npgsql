namespace pcap2latex.Templates.Paging;

public partial class PacketFooter
{
    public bool NewChapter { get; }
    public string SectionText { get; }
    public GenerationState State { get; }

    public PacketFooter(bool newChapter, GenerationState state, string sectionText = "Conversation")
    {
        this.NewChapter = newChapter;
        this.SectionText = sectionText;
        State = state;
    }

}
