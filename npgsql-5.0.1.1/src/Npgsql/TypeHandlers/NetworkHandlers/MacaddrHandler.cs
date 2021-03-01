using System.Diagnostics;
using System.Net.NetworkInformation;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.NetworkHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL macaddr and macaddr8 data types.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-net-types.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("macaddr8", EDBDbType.MacAddr8)]
    [TypeMapping("macaddr", EDBDbType.MacAddr, typeof(PhysicalAddress))]
    public class MacaddrHandler : EDBSimpleTypeHandler<PhysicalAddress>
    {
        /// <inheritdoc />
        public MacaddrHandler(PostgresType postgresType) : base(postgresType) {}

        #region Read

        /// <inheritdoc />
        public override PhysicalAddress Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            Debug.Assert(len == 6 || len == 8);

            var bytes = new byte[len];

            buf.ReadBytes(bytes, 0, len);
            return new PhysicalAddress(bytes);
        }

        #endregion Read

        #region Write

        /// <inheritdoc />
        public override int ValidateAndGetLength(PhysicalAddress value, EDBParameter? parameter)
            => value.GetAddressBytes().Length;

        /// <inheritdoc />
        public override void Write(PhysicalAddress value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            var bytes = value.GetAddressBytes();
            buf.WriteBytes(bytes, 0, bytes.Length);
        }

        #endregion Write
    }
}
