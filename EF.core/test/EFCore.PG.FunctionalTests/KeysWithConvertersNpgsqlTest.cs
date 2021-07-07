using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL
{
    public class KeysWithConvertersNpgsqlTest : KeysWithConvertersTestBase<
        KeysWithConvertersNpgsqlTest.KeysWithConvertersNpgsqlFixture>
    {
        public KeysWithConvertersNpgsqlTest(KeysWithConvertersNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class KeysWithConvertersNpgsqlFixture : KeysWithConvertersFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => NpgsqlTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => builder.UseNpgsql(b => b.MinBatchSize(1));
        }
    }
}
