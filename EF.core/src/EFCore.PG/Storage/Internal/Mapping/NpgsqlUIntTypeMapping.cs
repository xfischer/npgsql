using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlUintTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlUintTypeMapping([NotNull] string storeType, EDBDbType npgsqlDbType)
            : base(storeType, typeof(uint), npgsqlDbType) {}

        protected NpgsqlUintTypeMapping(RelationalTypeMappingParameters parameters, EDBDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlUintTypeMapping(parameters, EDBDbType);
    }
}
