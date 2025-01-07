namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

/// <summary>
///     Represents options for Npgsql that can only be set at the <see cref="IServiceProvider"/> singleton level.
/// </summary>
public interface INpgsqlSingletonOptions : ISingletonOptions
{
    /// <summary>
    ///     The backend version to target.
    /// </summary>
    Version PostgresVersion { get; }

    /// <summary>
    ///     The backend version to target, but returns <see langword="null" /> unless the user explicitly specified a version.
    /// </summary>
    bool IsPostgresVersionSet { get; }

    /// <summary>
    ///     Whether to target Redshift.
    /// </summary>
    bool UseRedshift { get; }

    /// <summary>
    ///     Whether reverse null ordering is enabled.
    /// </summary>
    bool ReverseNullOrderingEnabled { get; }

    /// <summary>
    ///     The collection of enum mappings.
    /// </summary>
    IReadOnlyList<EnumDefinition> EnumDefinitions { get; }

    /// <summary>
    ///     The collection of range mappings.
    /// </summary>
    IReadOnlyList<UserRangeDefinition> UserRangeDefinitions { get; }
}
