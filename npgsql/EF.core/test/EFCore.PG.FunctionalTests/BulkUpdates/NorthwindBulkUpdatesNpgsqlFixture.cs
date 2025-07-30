using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestModels.Northwind;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.BulkUpdates;

public class NorthwindBulkUpdatesNpgsqlFixture<TModelCustomizer> : NorthwindBulkUpdatesRelationalFixture<TModelCustomizer>
    where TModelCustomizer : ITestModelCustomizer, new()
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlNorthwindTestStoreFactory.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<MostExpensiveProduct>()
            .Property(p => p.UnitPrice)
            .HasColumnType("money");
    }

    protected override Type ContextType
        => typeof(NorthwindNpgsqlContext);
}
