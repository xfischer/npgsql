using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal;

public class IntervalRangeMapping : NpgsqlTypeMapping
{
    private static readonly ConstructorInfo _constructor =
        typeof(Interval).GetConstructor(new[] { typeof(Instant), typeof(Instant) })!;

    private static readonly ConstructorInfo _constructorWithNulls =
        typeof(Interval).GetConstructor(new[] { typeof(Instant?), typeof(Instant?) })!;

    public IntervalRangeMapping()
        : base("tstzrange", typeof(Interval), EDBDbType.TimestampTzRange)
    {
    }

    protected IntervalRangeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, EDBDbType.TimestampTzRange)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new IntervalRangeMapping(parameters);

    public override RelationalTypeMapping Clone(string storeType, int? size)
        => new IntervalRangeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

    public override CoreTypeMapping Clone(ValueConverter? converter)
        => new IntervalRangeMapping(Parameters.WithComposedConverter(converter));

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{GenerateEmbeddedNonNullSqlLiteral(value)}'::tstzrange";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
    {
        var interval = (Interval)value;

        var stringBuilder = new StringBuilder("[");

        if (interval.HasStart)
        {
            stringBuilder.Append(InstantPattern.ExtendedIso.Format(interval.Start));
        }

        stringBuilder.Append(',');

        if (interval.HasEnd)
        {
            stringBuilder.Append(InstantPattern.ExtendedIso.Format(interval.End));
        }

        stringBuilder.Append(')');

        return stringBuilder.ToString();
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        var interval = (Interval)value;

        return interval.HasStart && interval.HasEnd
            ? Expression.New(
                _constructor,
                TimestampTzInstantMapping.GenerateCodeLiteral(interval.Start),
                TimestampTzInstantMapping.GenerateCodeLiteral(interval.End))
            : Expression.New(
                _constructorWithNulls,
                interval.HasStart
                    ? Expression.Convert(
                        TimestampTzInstantMapping.GenerateCodeLiteral(interval.Start),
                        typeof(Instant?))
                    : Expression.Constant(null, typeof(Instant?)),
                interval.HasEnd
                    ? Expression.Convert(
                        TimestampTzInstantMapping.GenerateCodeLiteral(interval.End),
                        typeof(Instant?))
                    : Expression.Constant(null, typeof(Instant?)));
    }
}
