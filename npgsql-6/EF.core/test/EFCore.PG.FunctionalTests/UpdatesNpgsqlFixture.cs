using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL;

public class UpdatesNpgsqlFixture : UpdatesRelationalFixture
{
    protected override string StoreName { get; } = "PartialUpdateNpgsqlTest";
    protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
}