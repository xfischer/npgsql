using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// The type mapping for PostgreSQL range types.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/rangetypes.html
/// </remarks>
public class EDBRangeTypeMapping : NpgsqlTypeMapping
{
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static EDBRangeTypeMapping Default { get; } = new();

    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// The relational type mapping of the range's subtype.
    /// </summary>
    public virtual RelationalTypeMapping SubtypeMapping { get; }

    /// <summary>
    ///     For user-defined ranges, we have no <see cref="EDBDbType" /> and so the PG type name is set on
    ///     <see cref="EDBParameter.DataTypeName" /> instead.
    /// </summary>
    private string? PgDataTypeName { get; init; }

    /// <summary>
    ///     Constructs an instance of the <see cref="EDBRangeTypeMapping" /> class for a built-in range type which has a
    ///     <see cref="EDBDbType" /> defined.
    /// </summary>
    /// <param name="rangeStoreType">The database type to map</param>
    /// <param name="rangeClrType">The CLR type to map.</param>
    /// <param name="rangeEDBDbType">The <see cref="EDBDbType" /> of the built-in range.</param>
    /// <param name="subtypeMapping">The type mapping for the range subtype.</param>
    public static EDBRangeTypeMapping CreatBuiltInRangeMapping(
        string rangeStoreType,
        Type rangeClrType,
        EDBDbType rangeEDBDbType,
        RelationalTypeMapping subtypeMapping)
        => new(rangeStoreType, rangeClrType, rangeEDBDbType, subtypeMapping);

    /// <summary>
    ///     Constructs an instance of the <see cref="EDBRangeTypeMapping" /> class for a user-defined range type which doesn't have a
    ///     <see cref="EDBDbType" /> defined.
    /// </summary>
    /// <param name="quotedRangeStoreType">The database type to map, quoted.</param>
    /// <param name="unquotedRangeStoreType">The database type to map, unquoted.</param>
    /// <param name="rangeClrType">The CLR type to map.</param>
    /// <param name="subtypeMapping">The type mapping for the range subtype.</param>
    public static EDBRangeTypeMapping CreatUserDefinedRangeMapping(
        string quotedRangeStoreType,
        string unquotedRangeStoreType,
        Type rangeClrType,
        RelationalTypeMapping subtypeMapping)
        => new(quotedRangeStoreType, rangeClrType, rangeEDBDbType: EDBDbType.Unknown, subtypeMapping)
        {
            PgDataTypeName = unquotedRangeStoreType
        };

    private EDBRangeTypeMapping(
        string rangeStoreType,
        Type rangeClrType,
        EDBDbType rangeEDBDbType,
        RelationalTypeMapping subtypeMapping)
        : base(rangeStoreType, rangeClrType, rangeEDBDbType)
    {
        SubtypeMapping = subtypeMapping;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected EDBRangeTypeMapping(
        RelationalTypeMappingParameters parameters,
        EDBDbType npgsqlDbType,
        RelationalTypeMapping subtypeMapping)
        : base(parameters, npgsqlDbType)
    {
        SubtypeMapping = subtypeMapping;
    }

    // This constructor exists only to support the static Default property above, which is necessary to allow code generation for compiled
    // models. The constructor creates a completely blank type mapping, which will get cloned with all the correct details.
    private EDBRangeTypeMapping()
        : this("int4range", typeof(EDBRange<int>), EDBDbType.IntegerRange, subtypeMapping: null!)
    {
    }

    /// <summary>
    ///     This method exists only to support the compiled model.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual EDBRangeTypeMapping Clone(EDBDbType npgsqlDbType, RelationalTypeMapping subtypeTypeMapping)
        => new(Parameters, npgsqlDbType, subtypeTypeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new EDBRangeTypeMapping(parameters, EDBDbType, SubtypeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        // Built-in range types have an EDBDbType, so we just do the normal thing.
        if (PgDataTypeName is null)
        {
            Check.DebugAssert(EDBDbType is not EDBDbType.Unknown, "EDBDbType is Unknown but no PgDataTypeName is configured");
            base.ConfigureParameter(parameter);
            return;
        }

        Check.DebugAssert(EDBDbType is EDBDbType.Unknown, "PgDataTypeName is non-null, but EDBDbType is " + EDBDbType);

        if (parameter is not EDBParameter npgsqlParameter)
        {
            throw new InvalidOperationException(
                $"Npgsql-specific type mapping {GetType().Name} being used with non-Npgsql parameter type {parameter.GetType().Name}");
        }

        npgsqlParameter.DataTypeName = PgDataTypeName;
    }

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
