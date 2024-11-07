global using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDBBenchmark
{
    static class BenchmarkEnvironment
    {
        public const string ConnectionString = "Server=localhost;port=5446;User ID=enterprisedb;Password=edb;Database=test;Maximum Pool Size=200";

        internal static NpgsqlConnection GetConnection() => new(ConnectionString);

        internal static NpgsqlConnection OpenConnection()
        {
            var conn = GetConnection();
            conn.Open();
            return conn;
        }
    }
}
