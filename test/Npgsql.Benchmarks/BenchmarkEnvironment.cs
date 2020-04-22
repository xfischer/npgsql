using System;

namespace EnterpriseDB.EDBClient.Benchmarks
{
    static class BenchmarkEnvironment
    {
        internal static string ConnectionString => Environment.GetEnvironmentVariable("EDB_TEST_DB") ?? DefaultConnectionString;

        /// <summary>
        /// Unless the EDB_TEST_DB environment variable is defined, this is used as the connection string for the
        /// test database.
        /// </summary>
        const string DefaultConnectionString = "Server=localhost;User ID=EDB_tests;Password=EDB_tests;Database=EDB_tests";

        internal static EDBConnection GetConnection() => new EDBConnection(ConnectionString);

        internal static EDBConnection OpenConnection()
        {
            var conn = GetConnection();
            conn.Open();
            return conn;
        } 
    }
}
