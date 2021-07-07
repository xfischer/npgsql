using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    public class NpgsqlEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
    {
        [NotNull] static readonly MethodInfo TsQueryParse =
            typeof(EDBTsQuery).GetRuntimeMethod(nameof(EDBTsQuery.Parse), new[] { typeof(string) });

        [NotNull] static readonly MethodInfo TsVectorParse =
            typeof(EDBTsVector).GetRuntimeMethod(nameof(EDBTsVector.Parse), new[] { typeof(string) });

        public NpgsqlEvaluatableExpressionFilter(
            [NotNull] EvaluatableExpressionFilterDependencies dependencies,
            [NotNull] RelationalEvaluatableExpressionFilterDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {}

        public override bool IsEvaluatableExpression(Expression expression, IModel model)
        {
            if (expression is MethodCallExpression e)
            {
                var declaringType = e.Method.DeclaringType;

                if (e.Method == TsQueryParse || e.Method == TsVectorParse ||
                    declaringType == typeof(NpgsqlDbFunctionsExtensions) ||
                    declaringType == typeof(NpgsqlFullTextSearchDbFunctionsExtensions) ||
                    declaringType == typeof(NpgsqlFullTextSearchLinqExtensions) ||
                    declaringType == typeof(NpgsqlNetworkDbFunctionsExtensions) ||
                    declaringType == typeof(NpgsqlJsonDbFunctionsExtensions) ||
                    declaringType == typeof(EDBRangeDbFunctionsExtensions))
                {
                    return false;
                }
            }

            return base.IsEvaluatableExpression(expression, model);
        }
    }
}
