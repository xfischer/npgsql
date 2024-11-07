global using EnterpriseDB.EDBClient;
using System;

namespace Npgsql.Benchmarks;

static class BenchmarkEnvironment
{
    internal static string ConnectionString => Environment.GetEnvironmentVariable("NPGSQL_TEST_DB") ?? DefaultConnectionString;

    /// <summary>
    /// Unless the NPGSQL_TEST_DB environment variable is defined, this is used as the connection string for the
    /// test database.
    /// </summary>
    const string DefaultConnectionString = "Server=localhost;port=5446;User ID=enterprisedb;Password=edb;Database=test";

    internal static EDBConnection GetConnection() => new(ConnectionString);

    internal static EDBConnection OpenConnection()
    {
        var conn = GetConnection();
        conn.Open();
        return conn;
    } 
}
