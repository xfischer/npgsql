using System.Collections;
using System.Data.Common;
using System.Text;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// The type mapping for PostgreSQL multirange types.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/rangetypes.html
/// </remarks>
public class NpgsqlMultirangeTypeMapping : RelationalTypeMapping
{
    /// <summary>
    /// The relational type mapping of the ranges contained in this multirange.
    /// </summary>
    public virtual NpgsqlRangeTypeMapping RangeMapping
        => (NpgsqlRangeTypeMapping)ElementTypeMapping!;

    /// <summary>
    /// The relational type mapping of the values contained in this multirange.
    /// </summary>
    public virtual RelationalTypeMapping SubtypeMapping { get; }

    /// <summary>
    ///     The database type used by Npgsql.
    /// </summary>
    public virtual EDBDbType EDBDbType { get; }

    /// <summary>
    ///     Constructs an instance of the <see cref="NpgsqlRangeTypeMapping" /> class.
    /// </summary>
    /// <param name="storeType">The database type to map</param>
    /// <param name="clrType">The CLR type to map.</param>
    /// <param name="rangeMapping">The type mapping of the ranges contained in this multirange.</param>
    public NpgsqlMultirangeTypeMapping(string storeType, Type clrType, NpgsqlRangeTypeMapping rangeMapping)
        // TODO: Need to do comparer, converter
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(clrType, elementMapping: rangeMapping),
                storeType))
    {
        SubtypeMapping = rangeMapping.SubtypeMapping;
        EDBDbType = GenerateEDBDbType(rangeMapping.SubtypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlMultirangeTypeMapping(
        RelationalTypeMappingParameters parameters,
        EDBDbType npgsqlDbType)
        : base(parameters)
    {
        var rangeMapping = (NpgsqlRangeTypeMapping)parameters.CoreParameters.ElementTypeMapping!;

        SubtypeMapping = rangeMapping.SubtypeMapping;
        EDBDbType = npgsqlDbType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlMultirangeTypeMapping(parameters, EDBDbType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => GenerateNonNullSqlLiteral(value, RangeMapping, StoreType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string GenerateNonNullSqlLiteral(object value, RelationalTypeMapping rangeMapping, string multirangeStoreType)
    {
        var multirange = (IList)value;

        var sb = new StringBuilder();
        sb.Append("'{");

        for (var i = 0; i < multirange.Count; i++)
        {
            sb.Append(rangeMapping.GenerateEmbeddedSqlLiteral(multirange[i]));
            if (i < multirange.Count - 1)
            {
                sb.Append(", ");
            }
        }

        sb.Append("}'::");
        sb.Append(multirangeStoreType);
        return sb.ToString();
    }

    private static EDBDbType GenerateEDBDbType(RelationalTypeMapping subtypeMapping)
    {
        EDBDbType subtypeEDBDbType;
        if (subtypeMapping is INpgsqlTypeMapping npgsqlTypeMapping)
        {
            subtypeEDBDbType = npgsqlTypeMapping.EDBDbType;
        }
        else
        {
            // We're using a built-in, non-Npgsql mapping such as IntTypeMapping.
            // Infer the EDBDbType from the DbType (somewhat hacky but why not).
            Debug.Assert(subtypeMapping.DbType.HasValue);
            var p = new EDBParameter { DbType = subtypeMapping.DbType.Value };
            subtypeEDBDbType = p.EDBDbType;
        }

        return EDBDbType.Multirange | subtypeEDBDbType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
    {
        // Note that arrays are handled in EF Core's CSharpHelper, so this method doesn't get called for them.

        // Unfortunately, List<EDBRange<T>> requires MemberInit, which CSharpHelper doesn't support
        var type = value.GetType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            throw new NotSupportedException("Cannot generate code literals for List<T>, consider using arrays instead");
        }

        throw new InvalidCastException();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        if (parameter is not EDBParameter npgsqlParameter)
        {
            throw new ArgumentException(
                $"Npgsql-specific type mapping {GetType()} being used with non-Npgsql parameter type {parameter.GetType().Name}");
        }

        npgsqlParameter.EDBDbType = EDBDbType;
    }
}
