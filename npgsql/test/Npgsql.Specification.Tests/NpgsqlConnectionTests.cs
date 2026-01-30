using AdoNet.Specification.Tests;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.Specification.Tests;

public sealed class EDBConnectionTests : ConnectionTestBase<EDBDbFactoryFixture>
{
    public EDBConnectionTests(EDBDbFactoryFixture fixture)
        : base(fixture)
    {
    }

    public override Task OpenAsync_is_canceled() => Task.CompletedTask;
    public override void Dispose_raises_Disposed() { }

#if NET8_0_OR_GREATER
    public override Task DisposeAsync_raises_Disposed() => Task.CompletedTask;
#endif

}
