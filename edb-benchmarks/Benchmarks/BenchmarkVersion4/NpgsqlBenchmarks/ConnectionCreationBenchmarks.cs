using BenchmarkDotNet.Attributes;

// ReSharper disable UnusedMember.Global

namespace EDBBenchmark;

[Config(typeof(EDBManualConfig))]
public class ConnectionCreationBenchmarks
{
    const string EDBConnectionString = "Host=foo;Database=bar;Username=user;Password=password";
    //const string SqlClientConnectionString = @"Data Source=(localdb)\mssqllocaldb";

    [Benchmark]
    public EDBConnection EDB() => new(EDBConnectionString);

    //[Benchmark]
    //public SqlConnection SqlClient() => new(SqlClientConnectionString);

}