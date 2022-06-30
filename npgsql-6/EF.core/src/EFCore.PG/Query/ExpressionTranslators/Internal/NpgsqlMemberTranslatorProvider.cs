using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
/// A composite member translator that dispatches to multiple specialized member translators specific to EnterpriseDB.EDBClient.
/// </summary>
public class NpgsqlMemberTranslatorProvider : RelationalMemberTranslatorProvider
{
    public virtual NpgsqlJsonPocoTranslator JsonPocoTranslator { get; }

    public NpgsqlMemberTranslatorProvider(
        RelationalMemberTranslatorProviderDependencies dependencies,
        IModel model,
        IRelationalTypeMappingSource typeMappingSource,
        INpgsqlOptions npgsqlOptions)
        : base(dependencies)
    {
        var sqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
        JsonPocoTranslator = new NpgsqlJsonPocoTranslator(typeMappingSource, sqlExpressionFactory, model);

        AddTranslators(
            new IMemberTranslator[] {
                new NpgsqlArrayTranslator(sqlExpressionFactory, JsonPocoTranslator, npgsqlOptions.UseRedshift),
                new NpgsqlBigIntegerMemberTranslator(sqlExpressionFactory),
                new EDBDateTimeMemberTranslator(typeMappingSource, sqlExpressionFactory),
                new NpgsqlJsonDomTranslator(typeMappingSource, sqlExpressionFactory, model),
                new NpgsqlLTreeTranslator(typeMappingSource, sqlExpressionFactory, model),
                JsonPocoTranslator,
                new EDBRangeTranslator(typeMappingSource, sqlExpressionFactory, model),
                new NpgsqlStringMemberTranslator(sqlExpressionFactory),
                new NpgsqlTimeSpanMemberTranslator(sqlExpressionFactory),
            });
    }
}