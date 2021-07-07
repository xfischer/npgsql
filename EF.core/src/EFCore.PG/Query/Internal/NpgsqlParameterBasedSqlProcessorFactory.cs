using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    public class NpgsqlParameterBasedSqlProcessorFactory : IRelationalParameterBasedSqlProcessorFactory
    {
        readonly RelationalParameterBasedSqlProcessorDependencies _dependencies;

        public NpgsqlParameterBasedSqlProcessorFactory(
            [NotNull] RelationalParameterBasedSqlProcessorDependencies dependencies)
            => _dependencies = dependencies;

        public virtual RelationalParameterBasedSqlProcessor Create(bool useRelationalNulls)
            => new NpgsqlParameterBasedSqlProcessor(_dependencies, useRelationalNulls);
    }
}
