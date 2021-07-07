using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Utilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    public class NpgsqlQueryCompilationContextFactory : IQueryCompilationContextFactory
    {
        readonly QueryCompilationContextDependencies _dependencies;
        readonly RelationalQueryCompilationContextDependencies _relationalDependencies;

        public NpgsqlQueryCompilationContextFactory(
            [NotNull] QueryCompilationContextDependencies dependencies,
            [NotNull] RelationalQueryCompilationContextDependencies relationalDependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
        }

        public virtual QueryCompilationContext Create(bool async)
            => new NpgsqlQueryCompilationContext(_dependencies, _relationalDependencies, async);
    }
}
