using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL;

public class CompositeKeyEndToEndNpgsqlTest(CompositeKeyEndToEndNpgsqlTest.CompositeKeyEndToEndNpgsqlFixture fixture)
    : CompositeKeyEndToEndTestBase<CompositeKeyEndToEndNpgsqlTest.CompositeKeyEndToEndNpgsqlFixture>(fixture)
{
    public class CompositeKeyEndToEndNpgsqlFixture : CompositeKeyEndToEndFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}