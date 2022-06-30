using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL;

public class OverzealousInitializationNpgsqlTest
    : OverzealousInitializationTestBase<OverzealousInitializationNpgsqlTest.OverzealousInitializationNpgsqlFixture>
{
    public OverzealousInitializationNpgsqlTest(OverzealousInitializationNpgsqlFixture fixture)
        : base(fixture)
    {
    }

    public class OverzealousInitializationNpgsqlFixture : OverzealousInitializationFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
    }
}