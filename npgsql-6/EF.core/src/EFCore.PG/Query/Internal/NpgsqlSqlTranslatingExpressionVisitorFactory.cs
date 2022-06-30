using Microsoft.EntityFrameworkCore.Query;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.Internal;

public class NpgsqlSqlTranslatingExpressionVisitorFactory : IRelationalSqlTranslatingExpressionVisitorFactory
{
    private readonly RelationalSqlTranslatingExpressionVisitorDependencies _dependencies;

    public NpgsqlSqlTranslatingExpressionVisitorFactory(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies)
        => _dependencies = dependencies;

    public virtual RelationalSqlTranslatingExpressionVisitor Create(
        QueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        => new NpgsqlSqlTranslatingExpressionVisitor(
            _dependencies,
            queryCompilationContext,
            queryableMethodTranslatingExpressionVisitor);
}