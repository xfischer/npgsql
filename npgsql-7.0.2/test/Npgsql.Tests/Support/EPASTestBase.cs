using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests
{
    public abstract class EPASTestBase : TestBase
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            using var con = OpenConnection();
            TestUtil.EnsureEDBAdvancedServer(con);
        }
    }
}
