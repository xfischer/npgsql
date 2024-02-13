using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL;

public class NpgsqlServiceCollectionExtensionsTest : RelationalServiceCollectionExtensionsTestBase
{
    public NpgsqlServiceCollectionExtensionsTest()
        : base(NpgsqlTestHelpers.Instance)
    {
    }
}