using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class InheritanceQueryNpgsqlFixture : InheritanceQueryRelationalFixture
{
    protected override ITestStoreFactory TestStoreFactory =>  NpgsqlTestStoreFactory.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<AnimalQuery>().HasNoKey().ToSqlQuery(@"SELECT * FROM ""Animals""");
    }
}