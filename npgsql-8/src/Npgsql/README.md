The EDB .NET Connector is the .NET data provider for EDB Postgres Advanced Server. It allows you to connect and interact with EDB Postgres Advanced Server server using .NET.

## Quickstart

Here's a basic code snippet to get you started:

```csharp
var connString = "Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase";

await using var conn = new EDBConnection(connString);
await conn.OpenAsync();

// Insert some data
await using (var cmd = new EDBCommand("INSERT INTO data (some_field) VALUES (@p)", conn))
{
    cmd.Parameters.AddWithValue("p", "Hello world");
    await cmd.ExecuteNonQueryAsync();
}

// Retrieve all rows
await using (var cmd = new EDBCommand("SELECT some_field FROM data", conn))
await using (var reader = await cmd.ExecuteReaderAsync())
{
while (await reader.ReadAsync())
    Console.WriteLine(reader.GetString(0));
}
```

## Key features

* Full support of EnterpriseDB Postgres Advanced Server.
* Great integration with Entity Framework Core via [EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL](https://www.nuget.org/packages/EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL). 

For the full documentation, please visit [the EDB website](https://www.enterprisedb.com/docs/net_connector/latest/).

## Related packages

* The Entity Framework Core provider that works with this provider is [EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL](https://www.nuget.org/packages/EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL).
* Spatial plugin to work with EDB Postgres Advanced Server with PostGIS: [EnterpriseDB.EDBClient.NetTopologySuite](https://www.nuget.org/packages/EnterpriseDB.EDBClient.NetTopologySuite)
* NodaTime plugin to use better date/time types with EDB Postgres Advanced Server: [EnterpriseDB.EDBClient.NodaTime](https://www.nuget.org/packages/EnterpriseDB.EDBClient.NodaTime)
* OpenTelemetry support can be set up with [EnterpriseDB.EDBClient.OpenTelemetry](https://www.nuget.org/packages/EnterpriseDB.EDBClient.OpenTelemetry)