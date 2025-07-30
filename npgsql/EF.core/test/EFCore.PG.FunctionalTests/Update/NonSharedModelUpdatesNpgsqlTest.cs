using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Update;

public class NonSharedModelUpdatesNpgsqlTest : NonSharedModelUpdatesTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
