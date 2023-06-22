using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Internal;

/// <inheritdoc />
public class NpgsqlNetTopologySuiteOptions : INpgsqlNetTopologySuiteOptions
{
    /// <inheritdoc />
    public virtual bool IsGeographyDefault { get; set; }

    /// <inheritdoc />
    public virtual void Initialize(IDbContextOptions options)
    {
        var npgsqlNtsOptions = options.FindExtension<NpgsqlNetTopologySuiteOptionsExtension>() ?? new NpgsqlNetTopologySuiteOptionsExtension();

        IsGeographyDefault = npgsqlNtsOptions.IsGeographyDefault;
    }

    /// <inheritdoc />
    public virtual void Validate(IDbContextOptions options) {}
}