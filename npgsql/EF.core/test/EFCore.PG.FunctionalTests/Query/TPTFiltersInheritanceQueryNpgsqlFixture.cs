namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class TPTFiltersInheritanceQuerySqlServerFixture : TPTInheritanceQueryNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}