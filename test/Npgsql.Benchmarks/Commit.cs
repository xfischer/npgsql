using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;

namespace EnterpriseDB.EDBClient.Benchmarks
{
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [Config(typeof(Config))]
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

        class Config : ManualConfig
        {
            public Config()
            {
                Add(StatisticColumn.OperationsPerSecond);
            }
        }
    }
}
