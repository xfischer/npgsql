namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.BulkUpdates;

public class TPTFiltersInheritanceBulkUpdatesNpgsqlFixture : TPTInheritanceBulkUpdatesNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
