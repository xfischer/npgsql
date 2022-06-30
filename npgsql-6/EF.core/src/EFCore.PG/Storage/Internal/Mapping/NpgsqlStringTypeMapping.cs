using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// The base class for mapping Npgsql-specific string types. It configures parameters with the
/// <see cref="EDBDbType"/> provider-specific type enum.
/// </summary>
public class NpgsqlStringTypeMapping : StringTypeMapping, INpgsqlTypeMapping
{
    /// <inheritdoc />
    public virtual EDBDbType EDBDbType { get; }

    // ReSharper disable once PublicConstructorInAbstractClass
    /// <summary>
    /// Constructs an instance of the <see cref="NpgsqlTypeMapping"/> class.
    /// </summary>
    /// <param name="storeType">The database type to map.</param>
    /// <param name="EDBDbType">The database type used by EnterpriseDB.EDBClient.</param>
    public NpgsqlStringTypeMapping(string storeType, EDBDbType eDBDbType)
        : base(storeType, System.Data.DbType.String)
        => EDBDbType = eDBDbType;

    protected NpgsqlStringTypeMapping(
        RelationalTypeMappingParameters parameters,
        EDBDbType eDBDbType)
        : base(parameters)
        => EDBDbType = eDBDbType;

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlStringTypeMapping(parameters, EDBDbType);

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
