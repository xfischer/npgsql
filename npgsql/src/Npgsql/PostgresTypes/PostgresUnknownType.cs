using EnterpriseDB.EDBClient.Internal.Postgres;

namespace EnterpriseDB.EDBClient.PostgresTypes;

/// <summary>
/// Represents a PostgreSQL data type that isn't known to EDB and cannot be handled.
/// </summary>
public sealed class UnknownBackendType : PostgresType
{
    internal static readonly PostgresType Instance = new UnknownBackendType();

    /// <summary>
    /// Constructs a the unknown backend type.
    /// </summary>
    UnknownBackendType() : base(DataTypeName.Unspecified,0) { }
}