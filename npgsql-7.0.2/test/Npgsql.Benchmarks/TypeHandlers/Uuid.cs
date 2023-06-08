using System;
using BenchmarkDotNet.Attributes;
using EnterpriseDB.EDBClient.Internal.TypeHandlers;

namespace EnterpriseDB.EDBClient.Benchmarks.TypeHandlers;

[Config(typeof(Config))]
public class Uuid : TypeHandlerBenchmarks<Guid>
{
    public Uuid() : base(new UuidHandler(GetPostgresType("uuid"))) { }
}