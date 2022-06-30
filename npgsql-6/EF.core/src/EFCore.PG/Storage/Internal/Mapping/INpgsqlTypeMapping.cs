using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public interface INpgsqlTypeMapping
{
    /// <summary>
    /// The database type used by EnterpriseDB.EDBClient.
    /// </summary>
    EDBDbType EDBDbType { get; }
}
