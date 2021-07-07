using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public interface INpgsqlRelationalConnection : IRelationalConnection
    {
        INpgsqlRelationalConnection CreateMasterConnection();

        NpgsqlRelationalConnection CloneWith([NotNull] string connectionString);
    }
}
