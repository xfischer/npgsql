using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Design.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL;

public class DesignTimeNpgsqlTest(DesignTimeNpgsqlTest.DesignTimeNpgsqlFixture fixture)
    : DesignTimeTestBase<DesignTimeNpgsqlTest.DesignTimeNpgsqlFixture>(fixture)
{
    protected override Assembly ProviderAssembly
        => typeof(NpgsqlDesignTimeServices).Assembly;

    public class DesignTimeNpgsqlFixture : DesignTimeFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}