EDB .NET Connector is the .NET data provider for EDB Postgres Advanced Server. It allows you to connect and interact with EDB Postgres Advanced Server server using .NET.

This package helps set up EDB .NET Connector in applications using dependency injection, notably ASP.NET applications. It allows easy configuration of your EDB EPAS connections and registers the appropriate services in your DI container.

For example, if using the ASP.NET minimal web API, simply use the following to register EDB:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEDBDataSource("Host=pg_server;Username=test;Password=test;Database=test");
```

This registers a transient [`EDBConnection`](https://www.enterprisedb.com/docs/net_connector/latest/03_the_advanced_server_net_connector_overview/) which can get injected into your services or controllers:

```csharp
app.MapGet("/", async (EDBConnection connection) =>
{
    await connection.OpenAsync();
    await using var command = new EDBCommand("SELECT number FROM data LIMIT 1", connection);
    return "Hello World: " + await command.ExecuteScalarAsync();
});
```

An [`EDBDataSource`](https://www.enterprisedb.com/docs/net_connector/latest/03_the_advanced_server_net_connector_overview/) is also registered as a singleton:

```csharp
// Injected in service constructor
public SampleApplication(EDBDataSource dataSource)
{
    _dataSource = dataSource;
}

// Injected in an ASP.NET controller
app.MapGet("/", async (EDBDataSource dataSource) =>
{
    await using var command = dataSource.CreateCommand("SELECT number FROM data LIMIT 1");
    return "Hello World: " + await command.ExecuteScalarAsync();
});
```

Finally, the `AddEDBDataSource` method also accepts a lambda parameter allowing you to configure aspects of EDB beyond the connection string.

```csharp
var serviceProvider = new ServiceCollection()
    .AddEDBDataSource(connectionString, builder =>  // EDB .NET Connector injection point
    {
        builder.EnableParameterLogging(false);
        builder.MapComposite<MyComposite>();
    })                
    .BuildServiceProvider();
```

For more information, [see the EDB documentation](https://www.enterprisedb.com/docs/net_connector/latest/).
