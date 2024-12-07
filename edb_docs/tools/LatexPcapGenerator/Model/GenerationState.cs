namespace pcap2latex;

public class GenerationState
{
    public GenerationState(bool standalone)
    {
        Standalone = standalone;
    }

    public PostgresMessageBase? LastMessage { get; set; } = null;

    public int ConsecutiveDataRows { get; set; } = 0;
    public bool Standalone { get; }
    public float LatexRowCount { get; internal set; }
    public int StatsMesssagesProcessed { get; internal set; } = 0;
    public int StatsMesssagesInvalid { get; internal set; } = 0;
    public int StatsPacketsProcessed { get; internal set; } = 0;
}
