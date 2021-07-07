using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query
{
    public class OwnedQueryNpgsqlTest : OwnedQueryRelationalTestBase<OwnedQueryNpgsqlTest.OwnedQueryNpgsqlFixture>
    {
        public OwnedQueryNpgsqlTest(OwnedQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class OwnedQueryNpgsqlFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
