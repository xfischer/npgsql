using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Utilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Update.Internal
{
    public class NpgsqlModificationCommandBatchFactory : IModificationCommandBatchFactory
    {
        readonly ModificationCommandBatchFactoryDependencies _dependencies;
        readonly IDbContextOptions _options;

        public NpgsqlModificationCommandBatchFactory(
            [NotNull] ModificationCommandBatchFactoryDependencies dependencies,
            [NotNull] IDbContextOptions options)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(options, nameof(options));

            _dependencies = dependencies;
            _options = options;
        }

        public virtual ModificationCommandBatch Create()
        {
            var optionsExtension = _options.Extensions.OfType<NpgsqlOptionsExtension>().FirstOrDefault();

            return new NpgsqlModificationCommandBatch(_dependencies, optionsExtension?.MaxBatchSize);
        }
    }
}
