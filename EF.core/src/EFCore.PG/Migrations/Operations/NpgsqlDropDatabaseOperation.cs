using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Migrations.Operations
{
    public class NpgsqlDropDatabaseOperation : MigrationOperation
    {
        public virtual string Name { get;[param: NotNull] set; }
    }
}
