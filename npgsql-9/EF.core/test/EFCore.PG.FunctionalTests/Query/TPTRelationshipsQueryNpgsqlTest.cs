using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class TPTRelationshipsQueryNpgsqlTest
    : TPTRelationshipsQueryTestBase<TPTRelationshipsQueryNpgsqlTest.TPTRelationshipsQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public TPTRelationshipsQueryNpgsqlTest(
        TPTRelationshipsQueryNpgsqlFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    public class TPTRelationshipsQueryNpgsqlFixture : TPTRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}