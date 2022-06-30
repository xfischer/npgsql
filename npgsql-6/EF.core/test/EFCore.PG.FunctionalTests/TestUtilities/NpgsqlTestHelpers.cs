using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

public class NpgsqlTestHelpers : TestHelpers
{
    protected NpgsqlTestHelpers() {}

    public static NpgsqlTestHelpers Instance { get; } = new();

    public override IServiceCollection AddProviderServices(IServiceCollection services)
        => services.AddEntityFrameworkNpgsql();

    public override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(new EDBConnection("Host=localhost;Database=DummyDatabase"));

    public override LoggingDefinitions LoggingDefinitions { get; } = new NpgsqlLoggingDefinitions();
}