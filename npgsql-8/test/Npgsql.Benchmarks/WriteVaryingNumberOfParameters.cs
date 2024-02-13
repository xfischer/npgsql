using System.Linq;
using BenchmarkDotNet.Attributes;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Benchmarks.Types;

public class WriteVaryingNumberOfParameters
{
    EDBConnection _conn = default!;
    EDBCommand _cmd = default!;

    [Params(10)]
    public int NumParams { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _conn = BenchmarkEnvironment.OpenConnection();

        var funcParams = string.Join(",",
            Enumerable.Range(0, NumParams)
                .Select(i => $"IN p{i} int4")
        );
        using (var cmd = new EDBCommand($"CREATE FUNCTION pg_temp.swallow({funcParams}) RETURNS void AS 'BEGIN END;' LANGUAGE 'plpgsql'", _conn))
            cmd.ExecuteNonQuery();

        var cmdParams = string.Join(",", Enumerable.Range(0, NumParams).Select(i => $"@p{i}"));
        _cmd = new EDBCommand($"SELECT pg_temp.swallow({cmdParams})", _conn);
        for (var i = 0; i < NumParams; i++)
            _cmd.Parameters.Add(new EDBParameter("p" + i, EDBDbType.Integer));
        _cmd.Prepare();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cmd.Unprepare();
        _conn.Close();
    }

    [Benchmark]
    public void WriteParameters()
    {
        for (var i = 0; i < NumParams; i++)
            _cmd.Parameters[i].Value = i;
        _cmd.ExecuteNonQuery();
    }
}