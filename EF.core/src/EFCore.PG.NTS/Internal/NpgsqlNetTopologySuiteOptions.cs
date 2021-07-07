using Microsoft.EntityFrameworkCore.Infrastructure;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Internal
{
    /// <inheritdoc />
    public class NpgsqlNetTopologySuiteOptions : INpgsqlNetTopologySuiteOptions
    {
        /// <inheritdoc />
        public bool IsGeographyDefault { get; set; }

        /// <inheritdoc />
        public void Initialize(IDbContextOptions options)
        {
            var npgsqlNtsOptions = options.FindExtension<NpgsqlNetTopologySuiteOptionsExtension>() ?? new NpgsqlNetTopologySuiteOptionsExtension();

            IsGeographyDefault = npgsqlNtsOptions.IsGeographyDefault;
        }

        /// <inheritdoc />
        public void Validate(IDbContextOptions options) {}
    }
}
