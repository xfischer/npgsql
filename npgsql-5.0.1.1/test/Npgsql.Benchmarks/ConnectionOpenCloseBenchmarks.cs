using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using Microsoft.Data.SqlClient;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace EnterpriseDB.EDBClient.Benchmarks
{
    [Config(typeof(Config))]
    public class ConnectionOpenCloseBenchmarks
    {
        const string SqlClientConnectionString = @"Data Source=(localdb)\mssqllocaldb";

#pragma warning disable CS8618
        EDBCommand _noOpenCloseCmd;

        readonly string _openCloseConnString = new EDBConnectionStringBuilder(BenchmarkEnvironment.ConnectionString) { ApplicationName = nameof(OpenClose) }.ToString();
        readonly EDBCommand _openCloseCmd = new EDBCommand("SET lock_timeout = 1000");
        readonly SqlCommand _sqlOpenCloseCmd = new SqlCommand("SET LOCK_TIMEOUT 1000");

        EDBConnection _openCloseSameConn;
        EDBCommand _openCloseSameCmd;

        SqlConnection _sqlOpenCloseSameConn;
        SqlCommand _sqlOpenCloseSameCmd;

        EDBConnection _connWithPrepared;
        EDBCommand _withPreparedCmd;

        EDBConnection _noResetConn;
        EDBCommand _noResetCmd;

        EDBConnection _nonPooledConnection;
        EDBCommand _nonPooledCmd;
#pragma warning restore CS8618

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [Params(0, 1, 5, 10)]
        public int StatementsToSend { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var csb = new EDBConnectionStringBuilder(BenchmarkEnvironment.ConnectionString) { ApplicationName = nameof(NoOpenClose)};
            var noOpenCloseConn = new EDBConnection(csb.ToString());
            noOpenCloseConn.Open();
            _noOpenCloseCmd = new EDBCommand("SET lock_timeout = 1000", noOpenCloseConn);

            csb = new EDBConnectionStringBuilder(BenchmarkEnvironment.ConnectionString) { ApplicationName = nameof(OpenCloseSameConnection) };
            _openCloseSameConn = new EDBConnection(csb.ToString());
            _openCloseSameCmd = new EDBCommand("SET lock_timeout = 1000", _openCloseSameConn);

            _sqlOpenCloseSameConn = new SqlConnection(SqlClientConnectionString);
            _sqlOpenCloseSameCmd = new SqlCommand("SET LOCK_TIMEOUT 1000", _sqlOpenCloseSameConn);

            csb = new EDBConnectionStringBuilder(BenchmarkEnvironment.ConnectionString) { ApplicationName = nameof(WithPrepared) };
            _connWithPrepared = new EDBConnection(csb.ToString());
            _connWithPrepared.Open();
            using (var somePreparedCmd = new EDBCommand("SELECT 1", _connWithPrepared))
                somePreparedCmd.Prepare();
            _connWithPrepared.Close();
            _withPreparedCmd = new EDBCommand("SET lock_timeout = 1000", _connWithPrepared);

            csb = new EDBConnectionStringBuilder(BenchmarkEnvironment.ConnectionString)
            {
                ApplicationName = nameof(NoResetOnClose),
                NoResetOnClose = true
            };
            _noResetConn = new EDBConnection(csb.ToString());
            _noResetCmd = new EDBCommand("SET lock_timeout = 1000", _noResetConn);
            csb = new EDBConnectionStringBuilder(BenchmarkEnvironment.ConnectionString) {
                ApplicationName = nameof(NonPooled),
                Pooling = false
            };
            _nonPooledConnection = new EDBConnection(csb.ToString());
            _nonPooledCmd = new EDBCommand("SET lock_timeout = 1000", _nonPooledConnection);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _noOpenCloseCmd.Connection?.Close();
            EDBConnection.ClearAllPools();
            SqlConnection.ClearAllPools();
        }

        [Benchmark]
        public void NoOpenClose()
        {
            for (var i = 0; i < StatementsToSend; i++)
                _noOpenCloseCmd.ExecuteNonQuery();
        }

        [Benchmark]
        public void OpenClose()
        {
            using (var conn = new EDBConnection(_openCloseConnString))
            {
                conn.Open();
                _openCloseCmd.Connection = conn;
                for (var i = 0; i < StatementsToSend; i++)
                    _openCloseCmd.ExecuteNonQuery();
            }
        }

        [Benchmark(Baseline = true)]
        public void SqlClientOpenClose()
        {
            using (var conn = new SqlConnection(SqlClientConnectionString))
            {
                conn.Open();
                _sqlOpenCloseCmd.Connection = conn;
                for (var i = 0; i < StatementsToSend; i++)
                    _sqlOpenCloseCmd.ExecuteNonQuery();
            }
        }

        [Benchmark]
        public void OpenCloseSameConnection()
        {
            _openCloseSameConn.Open();
            for (var i = 0; i < StatementsToSend; i++)
                _openCloseSameCmd.ExecuteNonQuery();
            _openCloseSameConn.Close();
        }

        [Benchmark]
        public void SqlClientOpenCloseSameConnection()
        {
            _sqlOpenCloseSameConn.Open();
            for (var i = 0; i < StatementsToSend; i++)
                _sqlOpenCloseSameCmd.ExecuteNonQuery();
            _sqlOpenCloseSameConn.Close();
        }

        /// <summary>
        /// Having prepared statements alters the connection reset when closing.
        /// </summary>
        [Benchmark]
        public void WithPrepared()
        {
            _connWithPrepared.Open();
            for (var i = 0; i < StatementsToSend; i++)
                _withPreparedCmd.ExecuteNonQuery();
            _connWithPrepared.Close();
        }

        [Benchmark]
        public void NoResetOnClose()
        {
            _noResetConn.Open();
            for (var i = 0; i < StatementsToSend; i++)
                _noResetCmd.ExecuteNonQuery();
            _noResetConn.Close();
        }

        [Benchmark]
        public void NonPooled()
        {
            _nonPooledConnection.Open();
            for (var i = 0; i < StatementsToSend; i++)
                _nonPooledCmd.ExecuteNonQuery();
            _nonPooledConnection.Close();
        }

        class Config : ManualConfig
        {
            public Config()
            {
                AddColumn(StatisticColumn.OperationsPerSecond);
            }
        }
    }
}
