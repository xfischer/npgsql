using System;
using BenchmarkDotNet.Attributes;
using EnterpriseDB.EDBClient.Internal.Converters;

namespace Npgsql.Benchmarks.TypeHandlers;

/* EnterpriseDB: disabling tests, raises  System.NotSupportedException: Specified method is not supported
[Config(typeof(Config))]
public class Uuid : TypeHandlerBenchmarks<Guid>
{
    public Uuid() : base(new GuidUuidConverter()) { }
}
*/
