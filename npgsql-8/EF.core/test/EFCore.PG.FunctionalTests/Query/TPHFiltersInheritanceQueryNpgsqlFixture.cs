namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class TPHFiltersInheritanceQueryNpgsqlFixture : TPHInheritanceQueryNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
