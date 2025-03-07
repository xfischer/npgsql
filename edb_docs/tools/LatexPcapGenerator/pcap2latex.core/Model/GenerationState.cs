namespace pcap2latex;

public class GenerationState(bool standalone, bool multiple = false)
{
    public IPostgresMessage? LastMessage { get; set; } = null;

    public int ConsecutiveDataRows { get; set; } = 0;
    public bool Standalone { get; } = standalone;
    public bool Multiple { get; } = multiple;
    public float LatexRowCount { get; internal set; }
    public int StatsMesssagesProcessed { get; internal set; } = 0;
    public int StatsMesssagesInvalid { get; internal set; } = 0;
    public int StatsPacketsProcessed { get; internal set; } = 0;
}
