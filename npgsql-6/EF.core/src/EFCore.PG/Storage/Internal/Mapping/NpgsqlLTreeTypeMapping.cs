using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class NpgsqlLTreeTypeMapping : NpgsqlStringTypeMapping
{
    private static readonly ConstructorInfo Constructor = typeof(LTree).GetConstructor(new[] { typeof(string) })!;

    public NpgsqlLTreeTypeMapping()
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(LTree),
                    new ValueConverter<LTree, string>(l => l, s => new(s))),
                "ltree"),
            EDBDbType.LTree)
    {
    }

    protected NpgsqlLTreeTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, EDBDbType.LTree)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlLTreeTypeMapping(parameters);

    public override Expression GenerateCodeLiteral(object value)
        => Expression.New(Constructor, Expression.Constant((string)(LTree)value, typeof(string)));
}