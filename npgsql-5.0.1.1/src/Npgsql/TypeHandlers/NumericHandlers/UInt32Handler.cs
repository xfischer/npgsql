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
    /// See https://www.postgresql.org/docs/current/static/datatype-oid.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("oid", EDBDbType.Oid)]
    [TypeMapping("xid", EDBDbType.Xid)]
    [TypeMapping("cid", EDBDbType.Cid)]
    [TypeMapping("regtype", EDBDbType.Regtype)]
    [TypeMapping("regconfig", EDBDbType.Regconfig)]
    public class UInt32Handler : EDBSimpleTypeHandler<uint>
    {
        /// <inheritdoc />
        public UInt32Handler(PostgresType postgresType) : base(postgresType) {}

        /// <inheritdoc />
        public override uint Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => buf.ReadUInt32();

        /// <inheritdoc />
        public override int ValidateAndGetLength(uint value, EDBParameter? parameter) => 4;

        /// <inheritdoc />
        public override void Write(uint value, EDBWriteBuffer buf, EDBParameter? parameter)
            => buf.WriteUInt32(value);
    }
}
