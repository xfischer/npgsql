using Microsoft.EntityFrameworkCore.Storage;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class NpgsqlUintTypeMapping : NpgsqlTypeMapping
{
    public NpgsqlUintTypeMapping(string storeType, EDBDbType EDBDbType)
        : base(storeType, typeof(uint), EDBDbType) {}

    protected NpgsqlUintTypeMapping(RelationalTypeMappingParameters parameters, EDBDbType EDBDbType)
        : base(parameters, EDBDbType) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlUintTypeMapping(parameters, EDBDbType);
}