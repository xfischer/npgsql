namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class WarningsNpgsqlTest : WarningsTestBase<QueryNoClientEvalNpgsqlFixture>
{
    public WarningsNpgsqlTest(QueryNoClientEvalNpgsqlFixture fixture)
        : base(fixture)
    {
    }
}