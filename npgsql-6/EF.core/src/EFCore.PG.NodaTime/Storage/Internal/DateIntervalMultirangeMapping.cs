using System;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal;

public class DateIntervalMultirangeMapping : NpgsqlTypeMapping
{
    private readonly DateIntervalRangeMapping _dateIntervalRangeMapping;

    public DateIntervalMultirangeMapping(Type clrType, DateIntervalRangeMapping dateIntervalRangeMapping)
        : base("datemultirange", clrType, EDBDbType.DateMultirange)
        => _dateIntervalRangeMapping = dateIntervalRangeMapping;

    protected DateIntervalMultirangeMapping(RelationalTypeMappingParameters parameters, DateIntervalRangeMapping dateIntervalRangeMapping)
        : base(parameters, EDBDbType.DateMultirange)
        => _dateIntervalRangeMapping = dateIntervalRangeMapping;

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new DateIntervalMultirangeMapping(parameters, _dateIntervalRangeMapping);

    public override RelationalTypeMapping Clone(string storeType, int? size)
        => new DateIntervalMultirangeMapping(Parameters.WithStoreTypeAndSize(storeType, size), _dateIntervalRangeMapping);

    public override CoreTypeMapping Clone(ValueConverter? converter)
        => new DateIntervalMultirangeMapping(Parameters.WithComposedConverter(converter), _dateIntervalRangeMapping);

    protected override string GenerateNonNullSqlLiteral(object value)
        => NpgsqlMultirangeTypeMapping.GenerateNonNullSqlLiteral(value, _dateIntervalRangeMapping, "datemultirange");
}
