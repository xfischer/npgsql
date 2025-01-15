using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using EnterpriseDB.EDBClient;

// ReSharper disable AssignNullToNotNullAttribute.Global

namespace EDBBenchmark;

[Config(typeof(EDBManualConfig))]
public class Commit
{
    readonly EDBConnection _conn;
    readonly EDBCommand _cmd;

    public Commit()
    {
        _conn = BenchmarkEnvironment.OpenConnection();
        _cmd = new EDBCommand("SELECT 1", _conn);
    }

    [Benchmark]
    public void Basic()
    {
        var tx = _conn.BeginTransaction();
        _cmd.ExecuteNonQuery();
        tx.Commit();
    }

}
