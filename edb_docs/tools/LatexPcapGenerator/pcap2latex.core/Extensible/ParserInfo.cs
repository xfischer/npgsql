namespace pcap2latex;

public struct ParserInfo(PcapBinaryReader reader, PcapReadState state)
{    
    public PcapBinaryReader Reader { get; set; } = reader;
    public PcapReadState State { get; set; } = state;
    public bool IsFrontEnd { get; set; } = false;
    public ushort ClientPort { get; set; } = 0;
}
