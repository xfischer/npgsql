using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;

namespace EDBBenchmark;

[Config(typeof(EDBManualConfig))]
public class ReadRows
{
    [Params(1, 10, 100, 1000)]
    public int NumRows { get; set; }

    NpgsqlCommand Command { get; set; } = default!;

    [GlobalSetup]
    public void Setup()
    {
        var conn = BenchmarkEnvironment.OpenConnection();
        Command = new NpgsqlCommand($"SELECT generate_series(1, {NumRows})", conn);
        Command.Prepare();
    }

    [Benchmark]
    public void Read()
    {
        using (var reader = Command.ExecuteReader())
            while (reader.Read()) { }
    }

}
