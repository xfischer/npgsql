using AdoNet.Specification.Tests;
using Xunit;

namespace EnterpriseDB.EDBClient.Specification.Tests
{
    public sealed class EDBDataReaderTests : DataReaderTestBase<EDBSelectValueFixture>
    {
        public EDBDataReaderTests(EDBSelectValueFixture fixture)
            : base(fixture) {}
    }
}
