using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using Microsoft.Data.SqlClient;

// ReSharper disable UnusedMember.Global

namespace EnterpriseDB.EDBClient.Benchmarks
{
    [Config(typeof(Config))]
    public class ConnectionCreationBenchmarks
    {
        const string EDBConnectionString = "Host=foo;Database=bar;Username=user;Password=password";
        const string SqlClientConnectionString = @"Data Source=(localdb)\mssqllocaldb";

        [Benchmark]
        public EDBConnection EDB() => new EDBConnection(EDBConnectionString);

        [Benchmark]
        public SqlConnection SqlClient() => new SqlConnection(SqlClientConnectionString);

        class Config : ManualConfig
        {
            public Config()
            {
                Add(StatisticColumn.OperationsPerSecond);
            }
        }
    }
}
