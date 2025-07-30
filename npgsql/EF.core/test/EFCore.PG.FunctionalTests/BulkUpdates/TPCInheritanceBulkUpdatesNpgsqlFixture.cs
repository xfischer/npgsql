using Microsoft.EntityFrameworkCore.BulkUpdates;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.BulkUpdates;

public class TPCInheritanceBulkUpdatesNpgsqlFixture : TPCInheritanceBulkUpdatesFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    public override bool UseGeneratedKeys
        => false;
}
