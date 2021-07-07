using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Utilities;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class EDBTsQueryTypeMapping : NpgsqlTypeMapping
    {
        public EDBTsQueryTypeMapping() : base("tsquery", typeof(EDBTsQuery), EDBDbType.TsQuery) { }

        protected EDBTsQueryTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, EDBDbType.TsQuery) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new EDBTsQueryTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            Check.NotNull(value, nameof(value));
            var query = (EDBTsQuery)value;
            var builder = new StringBuilder();
            builder.Append("TSQUERY  ");
            var indexOfFirstQuote = builder.Length - 1;
            query.Write(builder, true);
            builder.Replace("'", "''");
            builder[indexOfFirstQuote] = '\'';
            builder.Append("'");
            return builder.ToString();
        }
    }
}
