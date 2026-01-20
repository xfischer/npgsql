using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using static EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlCubeTranslator(
    NpgsqlSqlExpressionFactory sqlExpressionFactory,
    IRelationalTypeMappingSource typeMappingSource) : IMethodCallTranslator, IMemberTranslator
{
    private readonly RelationalTypeMapping _cubeTypeMapping = typeMappingSource.FindMapping(typeof(EDBCube))!;
    private readonly RelationalTypeMapping _doubleTypeMapping = typeMappingSource.FindMapping(typeof(double))!;

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // Handle NpgsqlCubeDbFunctionsExtensions methods
        if (method.DeclaringType != typeof(NpgsqlCubeDbFunctionsExtensions))
        {
            return null;
        }

        return method.Name switch
        {
            nameof(NpgsqlCubeDbFunctionsExtensions.Overlaps) when arguments is [var cube1, var cube2]
                => sqlExpressionFactory.Overlaps(cube1, cube2),

            nameof(NpgsqlCubeDbFunctionsExtensions.Contains) when arguments is [var cube1, var cube2]
                => sqlExpressionFactory.Contains(cube1, cube2),

            nameof(NpgsqlCubeDbFunctionsExtensions.ContainedBy) when arguments is [var cube1, var cube2]
                => sqlExpressionFactory.ContainedBy(cube1, cube2),

            nameof(NpgsqlCubeDbFunctionsExtensions.Distance) when arguments is [var cube1, var cube2]
                => new PgBinaryExpression(
                    PgExpressionType.Distance,
                    sqlExpressionFactory.ApplyTypeMapping(cube1, _cubeTypeMapping),
                    sqlExpressionFactory.ApplyTypeMapping(cube2, _cubeTypeMapping),
                    typeof(double),
                    _doubleTypeMapping),

            nameof(NpgsqlCubeDbFunctionsExtensions.DistanceTaxicab) when arguments is [var cube1, var cube2]
                => new PgBinaryExpression(
                    PgExpressionType.CubeDistanceTaxicab,
                    sqlExpressionFactory.ApplyTypeMapping(cube1, _cubeTypeMapping),
                    sqlExpressionFactory.ApplyTypeMapping(cube2, _cubeTypeMapping),
                    typeof(double),
                    _doubleTypeMapping),

            nameof(NpgsqlCubeDbFunctionsExtensions.DistanceChebyshev) when arguments is [var cube1, var cube2]
                => new PgBinaryExpression(
                    PgExpressionType.CubeDistanceChebyshev,
                    sqlExpressionFactory.ApplyTypeMapping(cube1, _cubeTypeMapping),
                    sqlExpressionFactory.ApplyTypeMapping(cube2, _cubeTypeMapping),
                    typeof(double),
                    _doubleTypeMapping),

            nameof(NpgsqlCubeDbFunctionsExtensions.NthCoordinate) when arguments is [var cube, var index]
                => new PgBinaryExpression(
                    PgExpressionType.CubeNthCoordinate,
                    sqlExpressionFactory.ApplyTypeMapping(cube, _cubeTypeMapping),
                    ConvertToPostgresIndex(index),
                    typeof(double),
                    _doubleTypeMapping),

            nameof(NpgsqlCubeDbFunctionsExtensions.NthCoordinateKnn) when arguments is [var cube, var index]
                => new PgBinaryExpression(
                    PgExpressionType.CubeNthCoordinateKnn,
                    sqlExpressionFactory.ApplyTypeMapping(cube, _cubeTypeMapping),
                    ConvertToPostgresIndex(index),
                    typeof(double),
                    _doubleTypeMapping),

            nameof(NpgsqlCubeDbFunctionsExtensions.Union) when arguments is [var cube1, var cube2]
                => sqlExpressionFactory.Function(
                    "cube_union",
                    [cube1, cube2],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    typeof(EDBCube),
                    typeMappingSource.FindMapping(typeof(EDBCube))),

            nameof(NpgsqlCubeDbFunctionsExtensions.Intersect) when arguments is [var cube1, var cube2]
                => sqlExpressionFactory.Function(
                    "cube_inter",
                    [cube1, cube2],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    typeof(EDBCube),
                    typeMappingSource.FindMapping(typeof(EDBCube))),

            nameof(NpgsqlCubeDbFunctionsExtensions.Enlarge) when arguments is [var cube1, var cube2, var dimension]
                => sqlExpressionFactory.Function(
                    "cube_enlarge",
                    [cube1, cube2, dimension],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[3],
                    typeof(EDBCube),
                    typeMappingSource.FindMapping(typeof(EDBCube))),

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
        if (member.DeclaringType != typeof(EDBCube))
        {
            return null;
        }

        return member.Name switch
        {
            nameof(EDBCube.Dimensions)
                => sqlExpressionFactory.Function(
                    "cube_dim",
                    [instance!],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(int)),

            nameof(EDBCube.IsPoint)
                => sqlExpressionFactory.Function(
                    "cube_is_point",
                    [instance!],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(bool)),

            nameof(EDBCube.LowerLeft)
                => throw new InvalidOperationException(
                    $"The '{nameof(EDBCube.LowerLeft)}' property cannot be translated to SQL. " +
                    $"To access individual lower-left coordinates in queries, use indexer syntax (e.g., cube.LowerLeft[index]) instead."),

            nameof(EDBCube.UpperRight)
                => throw new InvalidOperationException(
                    $"The '{nameof(EDBCube.UpperRight)}' property cannot be translated to SQL. " +
                    $"To access individual upper-right coordinates in queries, use indexer syntax (e.g., cube.UpperRight[index]) instead."),

            _ => null
        };
    }

    /// <summary>
    /// Converts a zero-based index to one-based for PostgreSQL cube functions.
    /// For constant indexes, simplifies at translation time to avoid unnecessary addition in SQL.
    /// </summary>
    private SqlExpression ConvertToPostgresIndex(SqlExpression indexExpression)
    {
        var intTypeMapping = typeMappingSource.FindMapping(typeof(int));

        return indexExpression is SqlConstantExpression { Value: int index }
            ? sqlExpressionFactory.Constant(index + 1, intTypeMapping)
            : sqlExpressionFactory.Add(indexExpression, sqlExpressionFactory.Constant(1, intTypeMapping));
    }
}
