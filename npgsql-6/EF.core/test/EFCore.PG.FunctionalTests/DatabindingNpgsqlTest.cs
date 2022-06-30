using Microsoft.EntityFrameworkCore;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL;

public class DatabindingNpgsqlTest : DatabindingTestBase<F1NpgsqlFixture>
{
    public DatabindingNpgsqlTest(F1NpgsqlFixture fixture)
        : base(fixture)
    {
    }
}