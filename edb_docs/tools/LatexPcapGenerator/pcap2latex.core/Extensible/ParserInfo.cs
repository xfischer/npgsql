namespace pcap2latex;

public struct ParserInfo(PcapBinaryReader reader, PcapReadState state, bool isFrontEnd, ushort clientPort)
{    
    public PcapBinaryReader Reader { get; } = reader;
    public PcapReadState State { get; } = state;
    public bool  IsFrontEnd { get; } = isFrontEnd;
    public ushort ClientPort { get; } = clientPort;
}
