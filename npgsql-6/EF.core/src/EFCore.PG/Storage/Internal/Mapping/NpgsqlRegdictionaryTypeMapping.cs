using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class NpgsqlRegdictionaryTypeMapping : NpgsqlTypeMapping
{
    public NpgsqlRegdictionaryTypeMapping() : base("regdictionary", typeof(uint), EDBDbType.Oid) { }

    protected NpgsqlRegdictionaryTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, EDBDbType.Oid) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlRegdictionaryTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{EscapeSqlLiteral((string)value)}'";

    private string EscapeSqlLiteral(string literal)
        => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
}