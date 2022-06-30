using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class EDBDateTypeMapping : NpgsqlTypeMapping
{
    public EDBDateTypeMapping(Type clrType) : base("date", clrType, EDBDbType.Date) {}

    protected EDBDateTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, EDBDbType.Date) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new EDBDateTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"DATE '{GenerateEmbeddedNonNullSqlLiteral(value)}'";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
    {
        switch (value)
        {
            case DateTime dateTime:
                if (!NpgsqlTypeMappingSource.DisableDateTimeInfinityConversions)
                {
                    if (dateTime == DateTime.MinValue)
                    {
                        return "-infinity";
                    }

                    if (dateTime == DateTime.MaxValue)
                    {
                        return "infinity";
                    }
                }

                return dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            case DateOnly dateOnly:
                if (!NpgsqlTypeMappingSource.DisableDateTimeInfinityConversions)
                {
                    if (dateOnly == DateOnly.MinValue)
                    {
                        return "-infinity";
                    }

                    if (dateOnly == DateOnly.MaxValue)
                    {
                        return "infinity";
                    }
                }

                return dateOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            default:
                throw new InvalidCastException($"Can't generate a date SQL literal for CLR type {value.GetType()}");
        }
    }
}