using System;
using System.Data.Common;
using AdoNet.Specification.Tests;

namespace EnterpriseDB.EDBClient.Specification.Tests;

public class EDBDbFactoryFixture : IDbFactoryFixture
{
    public DbProviderFactory Factory => EDBFactory.Instance;

    const string DefaultConnectionString =
        "port=5444;Server=localhost;Username=enterprisedb;Password=edb;Database=test;Timeout=0;Command Timeout=0";

    public string ConnectionString =>
        Environment.GetEnvironmentVariable("NPGSQL_TEST_DB") ?? DefaultConnectionString;
}