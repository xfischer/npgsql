namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class InheritanceRelationshipsQueryNpgsqlTest : InheritanceRelationshipsQueryTestBase<InheritanceRelationshipsQueryNpgsqlFixture>
{
    public InheritanceRelationshipsQueryNpgsqlTest(InheritanceRelationshipsQueryNpgsqlFixture fixture)
        : base(fixture)
    {
    }

    protected override void ClearLog() {}
}