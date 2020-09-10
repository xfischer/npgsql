using System.Data;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL bool data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-boolean.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("boolean", EDBDbType.Boolean, DbType.Boolean, typeof(bool))]
    public class BoolHandler : EDBSimpleTypeHandler<bool>
    {
        /// <inheritdoc />
        public BoolHandler(PostgresType postgresType) : base(postgresType) {}

        /// <inheritdoc />
        public override bool Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => buf.ReadByte() != 0;

        /// <inheritdoc />
        public override int ValidateAndGetLength(bool value, EDBParameter? parameter)
            => 1;

        /// <inheritdoc />
        public override void Write(bool value, EDBWriteBuffer buf, EDBParameter? parameter)
            => buf.WriteByte(value ? (byte)1 : (byte)0);
    }
}
