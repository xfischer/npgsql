using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using EDBTypes;
using static EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

public class EDBRangeTranslator : IMethodCallTranslator, IMemberTranslator
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly IModel _model;

    private static readonly MethodInfo EnumerableAnyWithoutPredicate =
        typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(mi => mi.Name == nameof(Enumerable.Any) && mi.GetParameters().Length == 1);

    public EDBRangeTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        NpgsqlSqlExpressionFactory npgsqlSqlExpressionFactory,
        IModel model)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = npgsqlSqlExpressionFactory;
        _model = model;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // Any() over multirange -> NOT isempty(). EDBRange<T> has IsEmpty which is translated below.
        if (method.IsGenericMethod
            && method.GetGenericMethodDefinition() == EnumerableAnyWithoutPredicate
            && arguments[0].Type.TryGetMultirangeSubtype(out _))
        {
            return _sqlExpressionFactory.Not(
                _sqlExpressionFactory.Function(
                    "isempty",
                    new[] { arguments[0] },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(bool)));
        }

        if (method.DeclaringType != typeof(EDBRangeDbFunctionsExtensions)
            && method.DeclaringType != typeof(NpgsqlMultirangeDbFunctionsExtensions))
        {
            return null;
        }

        if (method.Name == nameof(EDBRangeDbFunctionsExtensions.Merge))
        {
            if (method.DeclaringType == typeof(EDBRangeDbFunctionsExtensions))
            {
                var inferredMapping = ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);

                return _sqlExpressionFactory.Function(
                    "range_merge",
                    new[] {
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[0], inferredMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[1], inferredMapping)
                    },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    method.ReturnType,
                    inferredMapping);
            }

            if (method.DeclaringType == typeof(NpgsqlMultirangeDbFunctionsExtensions))
            {
                var returnTypeMapping = arguments[0].TypeMapping is NpgsqlMultirangeTypeMapping multirangeTypeMapping
                    ? multirangeTypeMapping.RangeMapping
                    : null;

                return _sqlExpressionFactory.Function(
                    "range_merge",
                    new[] { arguments[0] },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    method.ReturnType,
                    returnTypeMapping);
            }
        }

        return method.Name switch
        {
            nameof(EDBRangeDbFunctionsExtensions.Contains)
                => _sqlExpressionFactory.Contains(arguments[0], arguments[1]),
            nameof(EDBRangeDbFunctionsExtensions.ContainedBy)
                => _sqlExpressionFactory.ContainedBy(arguments[0], arguments[1]),
            nameof(EDBRangeDbFunctionsExtensions.Overlaps)
                => _sqlExpressionFactory.Overlaps(arguments[0], arguments[1]),
            nameof(EDBRangeDbFunctionsExtensions.IsStrictlyLeftOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIsStrictlyLeftOf, arguments[0], arguments[1]),
            nameof(EDBRangeDbFunctionsExtensions.IsStrictlyRightOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIsStrictlyRightOf, arguments[0], arguments[1]),
            nameof(EDBRangeDbFunctionsExtensions.DoesNotExtendRightOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeDoesNotExtendRightOf, arguments[0], arguments[1]),
            nameof(EDBRangeDbFunctionsExtensions.DoesNotExtendLeftOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeDoesNotExtendLeftOf, arguments[0], arguments[1]),
            nameof(EDBRangeDbFunctionsExtensions.IsAdjacentTo)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIsAdjacentTo, arguments[0], arguments[1]),
            nameof(EDBRangeDbFunctionsExtensions.Union)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeUnion, arguments[0], arguments[1]),
            nameof(EDBRangeDbFunctionsExtensions.Intersect)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIntersect, arguments[0], arguments[1]),
            nameof(EDBRangeDbFunctionsExtensions.Except)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeExcept, arguments[0], arguments[1]),

            _ => null
        };
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var type = member.DeclaringType;
        if (type is null || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(EDBRange<>))
        {
            return null;
        }

        if (member.Name == nameof(EDBRange<int>.LowerBound) || member.Name == nameof(EDBRange<int>.UpperBound))
        {
            var typeMapping = instance!.TypeMapping is EDBRangeTypeMapping rangeMapping
                ? rangeMapping.SubtypeMapping
                : _typeMappingSource.FindMapping(returnType, _model);

            return _sqlExpressionFactory.Function(
                member.Name == nameof(EDBRange<int>.LowerBound) ? "lower" : "upper",
                new[] { instance },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                returnType,
                typeMapping);
        }

        return member.Name switch
        {
            nameof(EDBRange<int>.IsEmpty)               => SingleArgBoolFunction("isempty", instance!),
            nameof(EDBRange<int>.LowerBoundIsInclusive) => SingleArgBoolFunction("lower_inc", instance!),
            nameof(EDBRange<int>.UpperBoundIsInclusive) => SingleArgBoolFunction("upper_inc", instance!),
            nameof(EDBRange<int>.LowerBoundInfinite)    => SingleArgBoolFunction("lower_inf", instance!),
            nameof(EDBRange<int>.UpperBoundInfinite)    => SingleArgBoolFunction("upper_inf", instance!),

            _ => null
        };

        SqlFunctionExpression SingleArgBoolFunction(string name, SqlExpression argument)
            => _sqlExpressionFactory.Function(
                name,
                new[] { argument },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                typeof(bool));
    }

    private static readonly ConcurrentDictionary<Type, object> _defaults = new();

    private static object? GetDefaultValue(Type type)
        => type.IsValueType ? _defaults.GetOrAdd(type, Activator.CreateInstance!) : null;
}