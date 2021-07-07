namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query
{
    public class TPTFiltersInheritanceQuerySqlServerFixture : TPTInheritanceQueryNpgsqlFixture
    {
        protected override bool EnableFilters => true;
    }
}
