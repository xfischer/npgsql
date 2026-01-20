using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using EnterpriseDB.EDBClient.Internal.Postgres;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal;

static class AdoSerializerHelpers
{
    // EnterpriseDB: this PR https://github.com/npgsql/npgsql/pull/5768 assumes all text formats are unknown. This doesn't work with a mapped converter (ie TABLE OF types)
    // This method avoids raising an exception when called from RowDescriptionMessage.GetInfoSlow
    // See test: UnknownResultTypeList
    public static bool TryGetTypeInfoForReading(Type type, PgTypeId pgTypeId, PgSerializerOptions options, out PgTypeInfo? typeInfo)
    {
        try
        {
            typeInfo = options.GetTypeInfoInternal(type, pgTypeId);
            if (typeInfo is { SupportsReading: false })
                typeInfo = null;
        }
        catch
        {
            // EnterpriseDB: Exceptions are not happening in any tests, this is not meant to happen and is here for reliability
            typeInfo = null;
            return false;
        }

        return typeInfo != null;
    }

    public static PgTypeInfo GetTypeInfoForReading(Type type, PgTypeId pgTypeId, PgSerializerOptions options)
    {
        PgTypeInfo? typeInfo = null;
        Exception? inner = null;
        try
        {
            typeInfo = options.GetTypeInfoInternal(type, pgTypeId);
            if (typeInfo is { SupportsReading: false })
                typeInfo = null;
        }
        catch (Exception ex)
        {
            inner = ex;
        }
        return typeInfo ?? ThrowReadingNotSupported(type, options, pgTypeId, inner);

        // InvalidCastException thrown to align with ADO.NET convention.
        [DoesNotReturn]
        static PgTypeInfo ThrowReadingNotSupported(Type? type, PgSerializerOptions options, PgTypeId pgTypeId, Exception? inner = null)
        {
            throw new InvalidCastException(
                $"Reading{(type is null ? "" : $" as '{type.FullName}'")} is not supported for fields having DataTypeName '{options.DatabaseInfo.FindPostgresType(pgTypeId)?.DisplayName ?? "unknown"}'",
                inner);
        }
    }

    public static PgTypeInfo GetTypeInfoForWriting(Type? type, PgTypeId? pgTypeId, PgSerializerOptions options, EDBDbType? npgsqlDbType = null)
    {
        // EnterpriseDB: needed to comment for Output params and BindOut
        //Debug.Assert(type != typeof(object), "Parameters of type object are not supported.");

        PgTypeInfo? typeInfo = null;
        Exception? inner = null;
        try
        {
            typeInfo = options.GetTypeInfoInternal(type, pgTypeId);
            if (typeInfo is { SupportsWriting: false })
                typeInfo = null;
        }
        catch (Exception ex)
        {
            inner = ex;
        }
        return typeInfo ?? ThrowWritingNotSupported(type, options, pgTypeId, npgsqlDbType, inner);

        // InvalidCastException thrown to align with ADO.NET convention.
        [DoesNotReturn]
        static PgTypeInfo ThrowWritingNotSupported(Type? type, PgSerializerOptions options, PgTypeId? pgTypeId, EDBDbType? npgsqlDbType, Exception? inner = null)
        {
            var pgTypeString = pgTypeId is null
                ? "no EDBDbType or DataTypeName. Try setting one of these values to the expected database type."
                : npgsqlDbType is null
                    ? $"DataTypeName '{options.DatabaseInfo.FindPostgresType(pgTypeId.GetValueOrDefault())?.DisplayName ?? "unknown"}'"
                    : $"EDBDbType '{npgsqlDbType}'";

            throw new InvalidCastException(
                $"Writing{(type is null ? "" : $" values of '{type.FullName}'")} is not supported for parameters having {pgTypeString}.", inner);
        }
    }
}
