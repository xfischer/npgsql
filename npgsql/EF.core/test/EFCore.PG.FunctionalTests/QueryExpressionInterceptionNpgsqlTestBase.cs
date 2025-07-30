using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Infrastructure;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL;

public abstract class QueryExpressionInterceptionNpgsqlTestBase(
    QueryExpressionInterceptionNpgsqlTestBase.InterceptionNpgsqlFixtureBase fixture)
    : QueryExpressionInterceptionTestBase(fixture)
{
    public abstract class InterceptionNpgsqlFixtureBase : InterceptionFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkNpgsql(), injectedInterceptors);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            new NpgsqlDbContextOptionsBuilder(base.AddOptions(builder))
                .ExecutionStrategy(d => new NpgsqlExecutionStrategy(d));
            return builder;
        }
    }

    public class QueryExpressionInterceptionNpgsqlTest(QueryExpressionInterceptionNpgsqlTest.InterceptionNpgsqlFixture fixture)
        : QueryExpressionInterceptionNpgsqlTestBase(fixture), IClassFixture<QueryExpressionInterceptionNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
        {
            protected override string StoreName
                => "QueryExpressionInterception";

            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class QueryExpressionInterceptionWithDiagnosticsNpgsqlTest(
        QueryExpressionInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture fixture)
        : QueryExpressionInterceptionNpgsqlTestBase(fixture),
            IClassFixture<QueryExpressionInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture>
    {
        public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
        {
            protected override string StoreName
                => "QueryExpressionInterceptionWithDiagnostics";

            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
