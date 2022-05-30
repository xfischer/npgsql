using System.Collections;
using System.Threading.Tasks;
using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.Types
{
    [TestFixture(MultiplexingMode.NonMultiplexing, false)]
    [TestFixture(MultiplexingMode.NonMultiplexing, true)]
    [TestFixture(MultiplexingMode.Multiplexing, false)]
    [TestFixture(MultiplexingMode.Multiplexing, true)]
    public class LTxtQueryTests : TypeHandlerTestBase<string>
    {
        public LTxtQueryTests(MultiplexingMode multiplexingMode, bool useTypeName) : base(
            multiplexingMode,
            useTypeName ? null : EDBDbType.LTxtQuery,
            useTypeName ? "ltxtquery" : null)
        { }

        public static IEnumerable TestCases() => new[]
        {
            new object[] { "'Science & Astronomy'::ltxtquery", "Science & Astronomy" }
        };

        [OneTimeSetUp]
        public async Task SetUp()
        {
            using var conn = await OpenConnectionAsync();
            TestUtil.MinimumPgVersion(conn, "13.0");
            await TestUtil.EnsureExtensionAsync(conn, "ltree");
        }
    }
}
