using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using EDBTypes;
using EnterpriseDB.EDBClient;

namespace EDBBenchmark
{

    [Config(typeof(EDBManualConfig))]
    public class Insert
    {
        EDBConnection _Edbconn = default;
        EDBCommand _EdbtruncateCmd = default;

        //[Params(1, 100, 1000, 10000)]
        public int BatchSize { get; set; } = 2000;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var connString = new EDBConnectionStringBuilder(BenchmarkEnvironment.ConnectionString)
            {
                Pooling = false
            }.ToString();
            _Edbconn = new EDBConnection(connString);
            _Edbconn.Open();

            using (var cmd = new EDBCommand("CREATE TEMP TABLE data (int1 INT4, text1 TEXT, int2 INT4, text2 TEXT)", _Edbconn))
                cmd.ExecuteNonQuery();

            _EdbtruncateCmd = new EDBCommand("TRUNCATE data", _Edbconn);
        }

        [GlobalCleanup]
        public void GlobalCleanup() => _Edbconn.Close();

        [Benchmark(Baseline = true)]
        public void Unbatched()
        {
            var cmd = new EDBCommand("INSERT INTO data VALUES (@p0, @p1, @p2, @p3)", _Edbconn);
            cmd.Parameters.AddWithValue("p0", EDBDbType.Integer, 8);
            cmd.Parameters.AddWithValue("p1", EDBDbType.Text, "foo");
            cmd.Parameters.AddWithValue("p2", EDBDbType.Integer, 9);
            cmd.Parameters.AddWithValue("p3", EDBDbType.Text, "bar");
            cmd.Prepare();

            for (var i = 0; i < BatchSize; i++)
                cmd.ExecuteNonQuery();
            _EdbtruncateCmd.ExecuteNonQuery();
        }

        [Benchmark]
        [BenchmarkCategory("EDBPerfIssue")]
        public void Batched()
        {
            var cmd = new EDBCommand { Connection = _Edbconn };
            var sb = new StringBuilder();
            for (var i = 0; i < BatchSize; i++)
            {
                var p1 = (i * 4).ToString();
                var p2 = (i * 4 + 1).ToString();
                var p3 = (i * 4 + 2).ToString();
                var p4 = (i * 4 + 3).ToString();
                sb.Append("INSERT INTO data VALUES (@").Append(p1).Append(", @").Append(p2).Append(", @").Append(p3).Append(", @").Append(p4).Append(");");
                cmd.Parameters.AddWithValue(p1, EDBDbType.Integer, 8);
                cmd.Parameters.AddWithValue(p2, EDBDbType.Text, "foo");
                cmd.Parameters.AddWithValue(p3, EDBDbType.Integer, 9);
                cmd.Parameters.AddWithValue(p4, EDBDbType.Text, "bar");
            }
            cmd.CommandText = sb.ToString();
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            _EdbtruncateCmd.ExecuteNonQuery();
        }

        [Benchmark]
        public void Copy()
        {
            using (var s = _Edbconn.BeginBinaryImport("COPY data (int1, text1, int2, text2) FROM STDIN BINARY"))
            {
                for (var i = 0; i < BatchSize; i++)
                {
                    s.StartRow();
                    s.Write(8);
                    s.Write("foo");
                    s.Write(9);
                    s.Write("bar");
                }
            }
            _EdbtruncateCmd.ExecuteNonQuery();
        }
    }
}