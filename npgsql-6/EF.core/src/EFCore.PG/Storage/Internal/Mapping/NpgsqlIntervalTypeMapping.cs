using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class NpgsqlIntervalTypeMapping : NpgsqlTypeMapping
{
    public NpgsqlIntervalTypeMapping() : base("interval", typeof(TimeSpan), EDBDbType.Interval) {}

    protected NpgsqlIntervalTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, EDBDbType.Interval) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlIntervalTypeMapping(parameters);

    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null ? storeType : $"interval({parameters.Precision})";

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"INTERVAL '{FormatTimeSpanAsInterval((TimeSpan)value)}'";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $@"""{FormatTimeSpanAsInterval((TimeSpan)value)}""";

    public static string FormatTimeSpanAsInterval(TimeSpan ts)
        => ts.ToString(
            $@"{(ts < TimeSpan.Zero ? "\\-" : "")}{(ts.Days == 0 ? "" : "d\\ ")}hh\:mm\:ss{(ts.Ticks % 10000000 == 0 ? "" : "\\.FFFFFF")}",
            CultureInfo.InvariantCulture);
}