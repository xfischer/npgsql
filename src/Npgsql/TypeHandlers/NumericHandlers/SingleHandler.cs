using System.Data;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL real data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-numeric.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("real", EDBDbType.Real, DbType.Single, typeof(float))]
    public class SingleHandler : EDBSimpleTypeHandler<float>, IEDBSimpleTypeHandler<double>
    {
        /// <inheritdoc />
        public SingleHandler(PostgresType postgresType) : base(postgresType) {}

        #region Read

        /// <inheritdoc />
        public override float Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => buf.ReadSingle();

        double IEDBSimpleTypeHandler<double>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => Read(buf, len, fieldDescription);

        #endregion Read

        #region Write

        /// <inheritdoc />
        public int ValidateAndGetLength(double value, EDBParameter? parameter)         => 4;
        /// <inheritdoc />
        public override int ValidateAndGetLength(float value, EDBParameter? parameter) => 4;

        /// <inheritdoc />
        public void Write(double value, EDBWriteBuffer buf, EDBParameter? parameter)         => buf.WriteSingle((float)value);
        /// <inheritdoc />
        public override void Write(float value, EDBWriteBuffer buf, EDBParameter? parameter) => buf.WriteSingle(value);

        #endregion Write
    }
}
