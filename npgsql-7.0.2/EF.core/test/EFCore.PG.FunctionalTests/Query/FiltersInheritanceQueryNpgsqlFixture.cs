namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class FiltersInheritanceQueryNpgsqlFixture : InheritanceQueryNpgsqlFixture
{
    protected override bool EnableFilters => true;
}