using Microsoft.EntityFrameworkCore;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL
{
    public class PropertyEntryNpgsqlTest : PropertyEntryTestBase<F1NpgsqlFixture>
    {
        public PropertyEntryNpgsqlTest(F1NpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
