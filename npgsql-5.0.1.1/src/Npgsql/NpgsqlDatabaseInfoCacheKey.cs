using System;

namespace EnterpriseDB.EDBClient
{
    readonly struct EDBDatabaseInfoCacheKey : IEquatable<EDBDatabaseInfoCacheKey>
    {
        public readonly int Port;
        public readonly string? Host;
        public readonly string? Database;
        public readonly ServerCompatibilityMode CompatibilityMode;

        public EDBDatabaseInfoCacheKey(EDBConnectionStringBuilder connectionString)
        {
            Port = connectionString.Port;
            Host = connectionString.Host;
            Database = connectionString.Database;
            CompatibilityMode = connectionString.ServerCompatibilityMode;
        }

        public bool Equals(EDBDatabaseInfoCacheKey other) =>
            Port == other.Port &&
            Host == other.Host &&
            Database == other.Database &&
            CompatibilityMode == other.CompatibilityMode;

        public override bool Equals(object? obj) =>
            obj is EDBDatabaseInfoCacheKey key && key.Equals(this);

        public override int GetHashCode() =>
            Port.GetHashCode() ^
            Host?.GetHashCode() ?? 0 ^
            Database?.GetHashCode() ?? 0 ^
            CompatibilityMode.GetHashCode();
    }
}
