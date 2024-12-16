using System;
using EnterpriseDB.EDBClient;

var connectionString = Environment.GetEnvironmentVariable("NPGSQL_TEST_DB")
                       ?? "Server=localhost;Username=npgsql_tests;Password=npgsql_tests;Database=npgsql_tests;Timeout=0;Command Timeout=0";

var dataSourceBuilder = new EDBSlimDataSourceBuilder(connectionString);
await using var dataSource = dataSourceBuilder.Build();

await using var conn = dataSource.CreateConnection();
await conn.OpenAsync();
await using var cmd = new EDBCommand("SELECT 'Hello World'", conn);
await using var reader = await cmd.ExecuteReaderAsync();
if (!await reader.ReadAsync())
    throw new Exception("Got nothing from the database");

var value = reader.GetFieldValue<string>(0);
if (value != "Hello World")
    throw new Exception($"Got {value} instead of the expected 'Hello World'");
