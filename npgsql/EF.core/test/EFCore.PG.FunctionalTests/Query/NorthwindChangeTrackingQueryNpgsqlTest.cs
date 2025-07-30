using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestModels.Northwind;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class NorthwindChangeTrackingQueryNpgsqlTest : NorthwindChangeTrackingQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindChangeTrackingQueryNpgsqlTest(
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override NorthwindContext CreateNoTrackingContext()
        => new NorthwindNpgsqlContext(
            new DbContextOptionsBuilder(Fixture.CreateOptions())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);
}