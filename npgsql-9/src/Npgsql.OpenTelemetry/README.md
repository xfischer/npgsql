EDB .NET Connector is the .NET data provider for EDB Postgres Advanced Server. It allows you to connect and interact with EDB Postgres Advanced Server server using .NET.

This package helps set up EDB .NET Connector's support for OpenTelemetry tracing, which allows you to observe database commands as they are being executed.

You can drop the following code snippet in your application's startup, and you should start seeing tracing information on the console: 

```csharp
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("edb-dotnet-tester"))
    .SetSampler(new AlwaysOnSampler())
    // This optional activates tracing for your application, if you trace your own activities:
    .AddSource("MyApp")
    // This activates up Npgsql's tracing:
    .AddEDB()
    // This prints tracing data to the console:
    .AddConsoleExporter()
    .Build();
```

Once this is done, you should start seeing Npgsql trace data appearing in your application's console. At this point, you can look into exporting your trace data to a more useful destination: systems such as [Zipkin](https://zipkin.io/) or [Jaeger](https://www.jaegertracing.io/) can efficiently collect and store your data, and provide user interfaces for querying and exploring it.

For more information, [visit the diagnostics documentation page](https://www.npgsql.org/doc/diagnostics/tracing.html).
