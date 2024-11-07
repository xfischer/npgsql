using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
//using Microsoft.Data.SqlClient;

// ReSharper disable UnusedMember.Global

namespace EDBBenchmark;

[Config(typeof(EDBManualConfig))]
public class ConnectionCreationBenchmarks
{
    const string NpgsqlConnectionString = "Host=foo;Database=bar;Username=user;Password=password";
    //const string SqlClientConnectionString = @"Data Source=(localdb)\mssqllocaldb";

    [Benchmark]
    public NpgsqlConnection EDB() => new(NpgsqlConnectionString);

    //[Benchmark]
    //public SqlConnection SqlClient() => new(SqlClientConnectionString);

}