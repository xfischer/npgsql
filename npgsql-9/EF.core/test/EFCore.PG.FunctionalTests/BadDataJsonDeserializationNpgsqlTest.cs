#nullable enable

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL;

public class BadDataJsonDeserializationNpgsqlTest : BadDataJsonDeserializationTestBase
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => base.OnConfiguring(optionsBuilder.UseNpgsql(b => b.UseNetTopologySuite()));
}
