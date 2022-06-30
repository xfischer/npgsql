using System.Numerics;
using Microsoft.EntityFrameworkCore.Storage;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class NpgsqlBigIntegerTypeMapping : NpgsqlTypeMapping
{
    public NpgsqlBigIntegerTypeMapping() : base("numeric", typeof(BigInteger), EDBDbType.Numeric) {}

    protected NpgsqlBigIntegerTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, EDBDbType.Numeric)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlBigIntegerTypeMapping(parameters);

    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null
            ? storeType
            : parameters.Scale is null
                ? $"numeric({parameters.Precision})"
                : $"numeric({parameters.Precision},{parameters.Scale})";
}