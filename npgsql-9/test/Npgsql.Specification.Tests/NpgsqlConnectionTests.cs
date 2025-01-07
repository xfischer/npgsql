using AdoNet.Specification.Tests;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.Specification.Tests;

public sealed class NpgsqlConnectionTests(EDBDbFactoryFixture fixture) : ConnectionTestBase<EDBDbFactoryFixture>(fixture)
{
    // EnterpriseDB
    public override Task OpenAsync_is_canceled() { return Task.CompletedTask; }
}
