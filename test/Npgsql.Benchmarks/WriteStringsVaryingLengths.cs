using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Benchmarks.Types
{
    [Config(typeof(Config))]
    public class WriteStringsVaryingLengths
    {
        readonly EDBConnection _conn;
        EDBCommand _intCmd;
        EDBCommand _text1Cmd;
        EDBCommand _text100Cmd;
        EDBCommand _text1000Cmd;
        EDBCommand _text10000Cmd;

        #region Initialization

        public WriteStringsVaryingLengths()
        {
            _conn = BenchmarkEnvironment.OpenConnection();

            using (var cmd = new EDBCommand("CREATE TEMP TABLE foo (int INT, text TEXT)", _conn))
                cmd.ExecuteNonQuery();
            _intCmd = BuildCommand("int", EDBDbType.Integer, 8);
            _text1Cmd = BuildCommand("text", EDBDbType.Text, new string('x', 1));
            _text100Cmd = BuildCommand("text", EDBDbType.Text, new string('x', 100));
            _text1000Cmd = BuildCommand("text", EDBDbType.Text, new string('x', 1000));
            _text10000Cmd = BuildCommand("text", EDBDbType.Text, new string('x', 10000));
        }

        [GlobalSetup]
        public void Setup()
        {
            using (var cmd = new EDBCommand("TRUNCATE foo", _conn))
                cmd.ExecuteNonQuery();
        }

        EDBCommand BuildCommand(string column, EDBDbType EDBDbType, object value)
        {
            var cmd = new EDBCommand($"INSERT INTO foo ({column}) VALUES (@p)", _conn);
            cmd.Parameters.AddWithValue("p", EDBDbType, value);
            return cmd;
        }

        #endregion

        [Benchmark]
        public void Int()
        {
            _intCmd.ExecuteNonQuery();
        }

        [Benchmark]
        public void Text1()
        {
            _text1Cmd.ExecuteNonQuery();
        }

        [Benchmark]
        public void Text100()
        {
            _text100Cmd.ExecuteNonQuery();
        }

        [Benchmark]
        public void Text1000()
        {
            _text1000Cmd.ExecuteNonQuery();
        }

        [Benchmark]
        public void Text10000()
        {
            _text10000Cmd.ExecuteNonQuery();
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
