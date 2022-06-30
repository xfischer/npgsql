using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// Type mapping for the PostgreSQL 'character' data type. Handles both CLR strings and chars.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/datatype-character.html
/// </remarks>
public class NpgsqlCharacterCharTypeMapping : CharTypeMapping, INpgsqlTypeMapping
{
    /// <inheritdoc />
    public virtual EDBDbType EDBDbType
        => EDBDbType.Char;

    public NpgsqlCharacterCharTypeMapping(string storeType)
        : this(new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(typeof(char)),
            storeType,
            StoreTypePostfix.Size,
            System.Data.DbType.StringFixedLength,
            unicode: false,
            fixedLength: true)) {}

    protected NpgsqlCharacterCharTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlCharacterCharTypeMapping(parameters);

    protected override void ConfigureParameter(DbParameter parameter)
    {
        if (parameter is not EDBParameter EDBParameter)
        {
            throw new InvalidOperationException($"Npgsql-specific type mapping {GetType().Name} being used with non-Npgsql parameter type {parameter.GetType().Name}");
        }

        base.ConfigureParameter(parameter);
        EDBParameter.EDBDbType = EDBDbType;
    }
}
