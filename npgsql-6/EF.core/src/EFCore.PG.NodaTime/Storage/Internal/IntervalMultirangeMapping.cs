using System;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal;

public class IntervalMultirangeMapping : NpgsqlTypeMapping
{
    private readonly IntervalRangeMapping _intervalRangeMapping;

    public IntervalMultirangeMapping(Type clrType, IntervalRangeMapping intervalRangeMapping)
        : base("tstzmultirange", clrType, EDBDbType.TimestampTzMultirange)
        => _intervalRangeMapping = intervalRangeMapping;

    protected IntervalMultirangeMapping(RelationalTypeMappingParameters parameters, IntervalRangeMapping intervalRangeMapping)
        : base(parameters, EDBDbType.DateMultirange)
        => _intervalRangeMapping = intervalRangeMapping;

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new IntervalMultirangeMapping(parameters, _intervalRangeMapping);

    public override RelationalTypeMapping Clone(string storeType, int? size)
        => new IntervalMultirangeMapping(Parameters.WithStoreTypeAndSize(storeType, size), _intervalRangeMapping);

    public override CoreTypeMapping Clone(ValueConverter? converter)
        => new IntervalMultirangeMapping(Parameters.WithComposedConverter(converter), _intervalRangeMapping);

    protected override string GenerateNonNullSqlLiteral(object value)
        => NpgsqlMultirangeTypeMapping.GenerateNonNullSqlLiteral(value, _intervalRangeMapping, "tstzmultirange");
}
