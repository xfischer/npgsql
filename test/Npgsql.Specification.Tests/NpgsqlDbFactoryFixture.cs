using System;
using System.Data.Common;
using AdoNet.Specification.Tests;

namespace EnterpriseDB.EDBClient.Specification.Tests
{
    public class EDBDbFactoryFixture : IDbFactoryFixture
    {
        public DbProviderFactory Factory => EDBFactory.Instance;

        const string DefaultConnectionString =
            "Server=localhost;Username=EDB_tests;Password=EDB_tests;Database=EDB_tests;Timeout=0;Command Timeout=0";

        public string ConnectionString =>
            Environment.GetEnvironmentVariable("EDB_TEST_DB") ?? DefaultConnectionString;
    }
}
