namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class CompositeKeysQueryNpgsqlTest : CompositeKeysQueryRelationalTestBase<CompositeKeysQueryNpgsqlFixture>
{
    public CompositeKeysQueryNpgsqlTest(
        CompositeKeysQueryNpgsqlFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}