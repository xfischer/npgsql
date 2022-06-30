using Microsoft.EntityFrameworkCore.Storage;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal;

public interface INpgsqlRelationalConnection : IRelationalConnection
{
    INpgsqlRelationalConnection CreateMasterConnection();

    NpgsqlRelationalConnection CloneWith(string connectionString);
}