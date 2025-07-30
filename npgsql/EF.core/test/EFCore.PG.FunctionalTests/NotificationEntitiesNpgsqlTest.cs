using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL;

public class NotificationEntitiesNpgsqlTest(NotificationEntitiesNpgsqlTest.NotificationEntitiesNpgsqlFixture fixture)
    : NotificationEntitiesTestBase<NotificationEntitiesNpgsqlTest.NotificationEntitiesNpgsqlFixture>(fixture)
{
    public class NotificationEntitiesNpgsqlFixture : NotificationEntitiesFixtureBase
    {
        protected override string StoreName { get; } = "NotificationEntities";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}