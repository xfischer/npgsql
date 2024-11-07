using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using EDBBenchmark;

// ReSharper disable AssignNullToNotNullAttribute.Global

namespace EDBBenchmark;

[Config(typeof(EDBManualConfig))]
public class Commit
{
    readonly NpgsqlConnection _conn;
    readonly NpgsqlCommand _cmd;

    public Commit()
    {
        _conn = BenchmarkEnvironment.OpenConnection();
        _cmd = new NpgsqlCommand("SELECT 1", _conn);
    }

    [Benchmark]
    public void Basic()
    {
        var tx = _conn.BeginTransaction();
        _cmd.ExecuteNonQuery();
        tx.Commit();
    }

}
