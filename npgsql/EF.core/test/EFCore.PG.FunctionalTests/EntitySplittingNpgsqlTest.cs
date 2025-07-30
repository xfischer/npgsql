using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL;

public class EntitySplittingNpgsqlTest(ITestOutputHelper testOutputHelper) : EntitySplittingTestBase(testOutputHelper)
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
