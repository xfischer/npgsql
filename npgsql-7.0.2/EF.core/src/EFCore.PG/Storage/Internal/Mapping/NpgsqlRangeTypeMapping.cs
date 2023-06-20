using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// The type mapping for PostgreSQL range types.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/rangetypes.html
/// </remarks>
public class NpgsqlRangeTypeMapping : NpgsqlTypeMapping
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    private PropertyInfo? _isEmptyProperty;
    private PropertyInfo? _lowerProperty;
    private PropertyInfo? _upperProperty;
    private PropertyInfo? _lowerInclusiveProperty;
    private PropertyInfo? _upperInclusiveProperty;
    private PropertyInfo? _lowerInfiniteProperty;
    private PropertyInfo? _upperInfiniteProperty;

    private ConstructorInfo? _rangeConstructor1;
    private ConstructorInfo? _rangeConstructor2;
    private ConstructorInfo? _rangeConstructor3;

    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// The relational type mapping of the range's subtype.
    /// </summary>
    public virtual RelationalTypeMapping SubtypeMapping { get; }

    /// <summary>
    /// Constructs an instance of the <see cref="NpgsqlRangeTypeMapping"/> class.
    /// </summary>
    /// <param name="storeType">The database type to map</param>
    /// <param name="clrType">The CLR type to map.</param>
    /// <param name="subtypeMapping">The type mapping for the range subtype.</param>
    /// <param name="sqlGenerationHelper">The SQL generation helper to delimit the store name.</param>
    public NpgsqlRangeTypeMapping(
        string storeType,
        Type clrType,
        RelationalTypeMapping subtypeMapping,
        ISqlGenerationHelper sqlGenerationHelper)
        : this(storeType, storeTypeSchema: null, clrType, subtypeMapping, sqlGenerationHelper) {}

    /// <summary>
    /// Constructs an instance of the <see cref="NpgsqlRangeTypeMapping"/> class.
    /// </summary>
    /// <param name="storeType">The database type to map</param>
    /// <param name="storeTypeSchema">The schema of the type.</param>
    /// <param name="clrType">The CLR type to map.</param>
    /// <param name="subtypeMapping">The type mapping for the range subtype.</param>
    /// <param name="sqlGenerationHelper">The SQL generation helper to delimit the store name.</param>
    public NpgsqlRangeTypeMapping(
        string storeType,
        string? storeTypeSchema,
        Type clrType,
        RelationalTypeMapping subtypeMapping,
        ISqlGenerationHelper sqlGenerationHelper)
        : base(sqlGenerationHelper.DelimitIdentifier(storeType, storeTypeSchema), clrType, GenerateEDBDbType(subtypeMapping))
    {
        Debug.Assert(clrType == typeof(EDBRange<>).MakeGenericType(subtypeMapping.ClrType));

        SubtypeMapping = subtypeMapping;
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlRangeTypeMapping(
        RelationalTypeMappingParameters parameters,
        EDBDbType npgsqlDbType,
        RelationalTypeMapping subtypeMapping,
        ISqlGenerationHelper sqlGenerationHelper)
        : base(parameters, npgsqlDbType)
    {
        SubtypeMapping = subtypeMapping;
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlRangeTypeMapping(parameters, EDBDbType, SubtypeMapping, _sqlGenerationHelper);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{GenerateEmbeddedNonNullSqlLiteral(value)}'::{StoreType}";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
    {
        InitializeAccessors(ClrType, SubtypeMapping.ClrType);

        var builder = new StringBuilder();

        if ((bool)_isEmptyProperty.GetValue(value)!)
        {
            builder.Append("empty");
        }
        else
        {
            builder.Append((bool)_lowerInclusiveProperty.GetValue(value)! ? '[' : '(');

            if (!(bool)_lowerInfiniteProperty.GetValue(value)!)
            {
                builder.Append(SubtypeMapping.GenerateEmbeddedSqlLiteral(_lowerProperty.GetValue(value)));
            }

            builder.Append(',');

            if (!(bool)_upperInfiniteProperty.GetValue(value)!)
            {
                builder.Append(SubtypeMapping.GenerateEmbeddedSqlLiteral(_upperProperty.GetValue(value)));
            }

            builder.Append((bool)_upperInclusiveProperty.GetValue(value)! ? ']' : ')');
        }

        return builder.ToString();
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

        return EDBDbType.Range | subtypeEDBDbType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
    {
        InitializeAccessors(ClrType, SubtypeMapping.ClrType);

        var lower = _lowerProperty.GetValue(value);
        var upper = _upperProperty.GetValue(value);
        var lowerInclusive = (bool)_lowerInclusiveProperty.GetValue(value)!;
        var upperInclusive = (bool)_upperInclusiveProperty.GetValue(value)!;
        var lowerInfinite = (bool)_lowerInfiniteProperty.GetValue(value)!;
        var upperInfinite = (bool)_upperInfiniteProperty.GetValue(value)!;

        return lowerInfinite || upperInfinite
            ? Expression.New(
                _rangeConstructor3,
                Expression.Constant(lower),
                Expression.Constant(lowerInclusive),
                Expression.Constant(lowerInfinite),
                Expression.Constant(upper),
                Expression.Constant(upperInclusive),
                Expression.Constant(upperInfinite))
            : lowerInclusive && upperInclusive
                ? Expression.New(
                    _rangeConstructor1,
                    Expression.Constant(lower),
                    Expression.Constant(upper))
                : Expression.New(
                    _rangeConstructor2,
                    Expression.Constant(lower),
                    Expression.Constant(lowerInclusive),
                    Expression.Constant(upper),
                    Expression.Constant(upperInclusive));
    }

    [MemberNotNull(
        "_isEmptyProperty",
        "_lowerProperty", "_upperProperty",
        "_lowerInclusiveProperty", "_upperInclusiveProperty",
        "_lowerInfiniteProperty", "_upperInfiniteProperty",
        "_rangeConstructor1", "_rangeConstructor2", "_rangeConstructor3")]
    private void InitializeAccessors(Type rangeClrType, Type subtypeClrType)
    {
        _isEmptyProperty = rangeClrType.GetProperty(nameof(EDBRange<int>.IsEmpty))!;
        _lowerProperty = rangeClrType.GetProperty(nameof(EDBRange<int>.LowerBound))!;
        _upperProperty = rangeClrType.GetProperty(nameof(EDBRange<int>.UpperBound))!;
        _lowerInclusiveProperty = rangeClrType.GetProperty(nameof(EDBRange<int>.LowerBoundIsInclusive))!;
        _upperInclusiveProperty = rangeClrType.GetProperty(nameof(EDBRange<int>.UpperBoundIsInclusive))!;
        _lowerInfiniteProperty = rangeClrType.GetProperty(nameof(EDBRange<int>.LowerBoundInfinite))!;
        _upperInfiniteProperty = rangeClrType.GetProperty(nameof(EDBRange<int>.UpperBoundInfinite))!;

        _rangeConstructor1 = rangeClrType.GetConstructor(
            new[] { subtypeClrType, subtypeClrType })!;
        _rangeConstructor2 = rangeClrType.GetConstructor(
            new[] { subtypeClrType, typeof(bool), subtypeClrType, typeof(bool) })!;
        _rangeConstructor3 = rangeClrType.GetConstructor(
            new[] { subtypeClrType, typeof(bool), typeof(bool), subtypeClrType, typeof(bool), typeof(bool) })!;
    }
}
