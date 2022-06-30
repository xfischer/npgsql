using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// The base class for mapping Npgsql-specific types. It configures parameters with the
/// <see cref="EDBDbType"/> provider-specific type enum.
/// </summary>
public abstract class NpgsqlTypeMapping : RelationalTypeMapping, INpgsqlTypeMapping
{
    /// <inheritdoc />
    public virtual EDBDbType EDBDbType { get; }

    // ReSharper disable once PublicConstructorInAbstractClass
    /// <summary>
    /// Constructs an instance of the <see cref="NpgsqlTypeMapping"/> class.
    /// </summary>
    /// <param name="storeType">The database type to map.</param>
    /// <param name="clrType">The CLR type to map.</param>
    /// <param name="EDBDbType">The database type used by EnterpriseDB.EDBClient.</param>
    public NpgsqlTypeMapping(string storeType, Type clrType, EDBDbType eDBDbType)
        : base(storeType, clrType)
        => EDBDbType = eDBDbType;

    /// <summary>
    /// Constructs an instance of the <see cref="NpgsqlTypeMapping"/> class.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <param name="EDBDbType">The database type of the range subtype.</param>
    protected NpgsqlTypeMapping(RelationalTypeMappingParameters parameters, EDBDbType eDBDbType)
        : base(parameters)
        => EDBDbType = eDBDbType;

    protected override void ConfigureParameter(DbParameter parameter)
    {
        if (parameter is not EDBParameter EDBParameter)
        {
            throw new InvalidOperationException($"Npgsql-specific type mapping {GetType().Name} being used with non-Npgsql parameter type {parameter.GetType().Name}");
        }

        base.ConfigureParameter(parameter);
        EDBParameter.EDBDbType = EDBDbType;
    }

    /// <summary>
    /// Generates the SQL representation of a literal value meant to be embedded in another literal value, e.g. in a range.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>
    /// The generated string.
    /// </returns>
    public virtual string GenerateEmbeddedSqlLiteral(object? value)
    {
        value = ConvertUnderlyingEnumValueToEnum(value);

        if (Converter != null)
        {
            value = Converter.ConvertToProvider(value);
        }

        return GenerateEmbeddedProviderValueSqlLiteral(value);
    }

    /// <summary>
    /// Generates the SQL representation of a literal value without conversion, meant to be embedded in another literal value,
    /// e.g. in a range.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>
    /// The generated string.
    /// </returns>
    public virtual string GenerateEmbeddedProviderValueSqlLiteral(object? value)
        => value == null
            ? "NULL"
            : GenerateEmbeddedNonNullSqlLiteral(value);

    /// <summary>
    /// Generates the SQL representation of a non-null literal value, meant to be embedded in another literal value, e.g. in a range.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>
    /// The generated string.
    /// </returns>
    protected virtual string GenerateEmbeddedNonNullSqlLiteral(object value)
        => GenerateNonNullSqlLiteral(value);

    // Copied from RelationalTypeMapping
    private object? ConvertUnderlyingEnumValueToEnum(object? value)
        => value?.GetType().IsInteger() == true && ClrType.UnwrapNullableType().IsEnum
            ? Enum.ToObject(ClrType.UnwrapNullableType(), value)
            : value;
}
