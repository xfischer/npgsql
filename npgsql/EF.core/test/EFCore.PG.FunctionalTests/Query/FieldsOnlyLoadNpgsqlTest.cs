using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class FieldsOnlyLoadNpgsqlTest(FieldsOnlyLoadNpgsqlTest.FieldsOnlyLoadNpgsqlFixture fixture)
    : FieldsOnlyLoadTestBase<FieldsOnlyLoadNpgsqlTest.FieldsOnlyLoadNpgsqlFixture>(fixture)
{
    public class FieldsOnlyLoadNpgsqlFixture : FieldsOnlyLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}