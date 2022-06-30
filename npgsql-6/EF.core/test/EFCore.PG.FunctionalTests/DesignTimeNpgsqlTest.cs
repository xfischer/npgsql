using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Design.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL;

public class DesignTimeNpgsqlTest : DesignTimeTestBase<DesignTimeNpgsqlTest.DesignTimeNpgsqlFixture>
{
    public DesignTimeNpgsqlTest(DesignTimeNpgsqlFixture fixture)
        : base(fixture)
    {
    }

    protected override Assembly ProviderAssembly
        => typeof(NpgsqlDesignTimeServices).Assembly;

    public class DesignTimeNpgsqlFixture : DesignTimeFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}