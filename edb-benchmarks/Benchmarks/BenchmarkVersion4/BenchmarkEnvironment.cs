using EnterpriseDB.EDBClient;
#if NPGSQL
using Npgsql;
#endif
namespace EDBBenchmark
{
    static class BenchmarkEnvironment
    {
        public const string ConnectionString = "Server=localhost;port=5447;User ID=enterprisedb;Password=edb;Database=test;Maximum Pool Size=200";

        internal static EDBConnection GetConnection() => new EDBConnection(ConnectionString);

        internal static EDBConnection OpenConnection()
        {
            var conn = GetConnection();
            conn.Open();
            return conn;
        }

#if NPGSQL
        internal static NpgsqlConnection GetNpgsqlConnection() => new(ConnectionString);

        internal static NpgsqlConnection OpenNpgsqlConnection()
        {
            var conn = GetNpgsqlConnection();
            conn.Open();
            return conn;
        }
#endif
    }
}
