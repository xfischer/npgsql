using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Text;
using EnterpriseDB.EDBClient.Internal.Converters;

namespace Npgsql.Benchmarks.TypeHandlers;

/* EnterpriseDB: disabling tests, raises  System.NotSupportedException: Specified method is not supported
[Config(typeof(Config))]
public class Text : TypeHandlerBenchmarks<string>
{
    public Text() : base(new StringTextConverter(Encoding.UTF8)) { }

    protected override IEnumerable<string> ValuesOverride()
    {
        for (var i = 1; i <= 10000; i *= 10)
            yield return new string('x', i);
    }
}
*/
