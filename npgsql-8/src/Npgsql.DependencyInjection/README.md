EDB is the open source .NET data provider for PostgreSQL. It allows you to connect and interact with PostgreSQL server using .NET.

This package helps set up EDB in applications using dependency injection, notably ASP.NET applications. It allows easy configuration of your EDB connections and registers the appropriate services in your DI container. 

For example, if using the ASP.NET minimal web API, simply use the following to register EDB:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEDBDataSource("Host=pg_server;Username=test;Password=test;Database=test");
```

This registers a transient [`EDBConnection`](https://www.npgsql.org/doc/api/EnterpriseDB.EDBClient.EDBConnection.html) which can get injected into your controllers:

```csharp
app.MapGet("/", async (EDBConnection connection) =>
{
    await connection.OpenAsync();
    await using var command = new EDBCommand("SELECT number FROM data LIMIT 1", connection);
    return "Hello World: " + await command.ExecuteScalarAsync();
});
```

But wait! If all you want is to execute some simple SQL, just use the singleton [`EDBDataSource`](https://www.npgsql.org/doc/api/EnterpriseDB.EDBClient.EDBDataSource.html) to execute a command directly:

```csharp
app.MapGet("/", async (EDBDataSource dataSource) =>
{
    await using var command = dataSource.CreateCommand("SELECT number FROM data LIMIT 1");
    return "Hello World: " + await command.ExecuteScalarAsync();
});
```

[`EDBDataSource`](https://www.npgsql.org/doc/api/EnterpriseDB.EDBClient.EDBDataSource.html) can also come in handy when you need more than one connection:

```csharp
app.MapGet("/", async (EDBDataSource dataSource) =>
{
    await using var connection1 = await dataSource.OpenConnectionAsync();
    await using var connection2 = await dataSource.OpenConnectionAsync();
    // Use the two connections...
});
```

The `AddEDBDataSource` method also accepts a lambda parameter allowing you to configure aspects of EDB beyond the connection string, e.g. to configure `UseLoggerFactory` and `UseNetTopologySuite`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEDBDataSource(
    "Host=pg_server;Username=test;Password=test;Database=test",
    builder => builder
        .UseLoggerFactory(loggerFactory)
        .UseNetTopologySuite());
```

Finally, starting with EDB and .NET 8.0, you can now register multiple data sources (and connections), using a service key to distinguish between them:

```c#
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEDBDataSource("Host=localhost;Database=CustomersDB;Username=test;Password=test", serviceKey: DatabaseType.CustomerDb)
    .AddEDBDataSource("Host=localhost;Database=OrdersDB;Username=test;Password=test", serviceKey: DatabaseType.OrdersDb);

var app = builder.Build();

app.MapGet("/", async ([FromKeyedServices(DatabaseType.OrdersDb)] EDBConnection connection)
    => connection.ConnectionString);

app.Run();

enum DatabaseType
{
    CustomerDb,
    OrdersDb
}
```

For more information, [see the EDB documentation](https://www.npgsql.org/doc/index.html).
