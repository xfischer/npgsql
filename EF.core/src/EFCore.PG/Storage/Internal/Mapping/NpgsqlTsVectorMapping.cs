using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Utilities;
using EDBTypes;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class EDBTsVectorTypeMapping : NpgsqlTypeMapping
    {
        public EDBTsVectorTypeMapping() : base("tsvector", typeof(EDBTsVector), EDBDbType.TsVector) { }

        protected EDBTsVectorTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, EDBDbType.TsVector) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new EDBTsVectorTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            Check.NotNull(value, nameof(value));
            var vector = (EDBTsVector)value;
            var builder = new StringBuilder();
            builder.Append("TSVECTOR  ");
            var indexOfFirstQuote = builder.Length - 1;
            builder.Append(vector);
            builder.Replace("'", "''");
            builder[indexOfFirstQuote] = '\'';
            builder.Append("'");
            return builder.ToString();
        }
    }
}
