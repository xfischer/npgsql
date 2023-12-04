using AdoNet.Specification.Tests;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.Specification.Tests;

public sealed class EDBConnectionTests : ConnectionTestBase<EDBDbFactoryFixture>
{
    public EDBConnectionTests(EDBDbFactoryFixture fixture)
        : base(fixture)
    {
    }

    // EnterpriseDB
    public override Task OpenAsync_is_canceled() { return Task.CompletedTask; }
}
