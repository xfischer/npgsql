using AdoNet.Specification.Tests;

namespace EnterpriseDB.EDBClient.Specification.Tests
{
    public sealed class EDBConnectionTests : ConnectionTestBase<EDBDbFactoryFixture>
    {
        public EDBConnectionTests(EDBDbFactoryFixture fixture)
            : base(fixture)
        {
        }
    }
}
