using System.Data;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL double precision data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-numeric.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("double precision", EDBDbType.Double, DbType.Double, typeof(double))]
    public class DoubleHandler : EDBSimpleTypeHandler<double>
    {
        /// <inheritdoc />
        public DoubleHandler(PostgresType postgresType) : base(postgresType) {}

        /// <inheritdoc />
        public override double Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => buf.ReadDouble();

        /// <inheritdoc />
        public override int ValidateAndGetLength(double value, EDBParameter? parameter)
            => 8;

        /// <inheritdoc />
        public override void Write(double value, EDBWriteBuffer buf, EDBParameter? parameter)
            => buf.WriteDouble(value);
    }
}
