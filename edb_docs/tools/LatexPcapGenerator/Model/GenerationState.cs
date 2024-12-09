namespace pcap2latex;

public class GenerationState
{
    public GenerationState(bool standalone, bool multiple = false)
    {
        Standalone = standalone;
        Multiple = multiple;
    }

    public PostgresMessageBase? LastMessage { get; set; } = null;

    public int ConsecutiveDataRows { get; set; } = 0;
    public bool Standalone { get; }
    public bool Multiple { get; }
    public float LatexRowCount { get; internal set; }
    public int StatsMesssagesProcessed { get; internal set; } = 0;
    public int StatsMesssagesInvalid { get; internal set; } = 0;
    public int StatsPacketsProcessed { get; internal set; } = 0;
}
