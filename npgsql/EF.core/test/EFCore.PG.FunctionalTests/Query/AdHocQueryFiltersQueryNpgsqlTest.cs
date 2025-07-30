using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class AdHocQueryFiltersQueryNpgsqlTest : AdHocQueryFiltersQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
