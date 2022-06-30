using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

public interface INpgsqlArrayConverter
{
    ValueConverter ElementConverter { get; }
}