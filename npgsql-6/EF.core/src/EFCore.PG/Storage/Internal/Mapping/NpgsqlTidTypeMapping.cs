using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class EDBTidTypeMapping : NpgsqlTypeMapping
{
    public EDBTidTypeMapping()
        : base("tid", typeof(EDBTid), EDBDbType.Tid) {}

    protected EDBTidTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, EDBDbType.Tid) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new EDBTidTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var tid = (EDBTid)value;
        var builder = new StringBuilder("TID '(");
        builder.Append(tid.BlockNumber);
        builder.Append(',');
        builder.Append(tid.OffsetNumber);
        builder.Append(")'");
        return builder.ToString();
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        var tid = (EDBTid)value;
        return Expression.New(Constructor, Expression.Constant(tid.BlockNumber), Expression.Constant(tid.OffsetNumber));
    }

    private static readonly ConstructorInfo Constructor =
        typeof(EDBTid).GetConstructor(new[] { typeof(uint), typeof(ushort) })!;
}