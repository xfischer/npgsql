using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class FieldsOnlyLoadNpgsqlTest : FieldsOnlyLoadTestBase<FieldsOnlyLoadNpgsqlTest.FieldsOnlyLoadNpgsqlFixture>
{
    public FieldsOnlyLoadNpgsqlTest(FieldsOnlyLoadNpgsqlFixture fixture)
        : base(fixture)
    {
    }

    public class FieldsOnlyLoadNpgsqlFixture : FieldsOnlyLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}