using BenchmarkDotNet.Attributes;

namespace EnterpriseDB.EDBClient.Benchmarks;

public class ReadRows
{
    [Params(1, 10, 100, 1000)]
    public int NumRows { get; set; }

    EDBCommand Command { get; set; } = default!;

    [GlobalSetup]
    public void Setup()
    {
        var conn = BenchmarkEnvironment.OpenConnection();
        Command = new EDBCommand($"SELECT generate_series(1, {NumRows})", conn);
        Command.Prepare();
    }

    [Benchmark]
    public void Read()
    {
        using (var reader = Command.ExecuteReader())
            while (reader.Read()) { }
    }
}