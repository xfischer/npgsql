using System.Net;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

#pragma warning disable 618

namespace EnterpriseDB.EDBClient.TypeHandlers.NetworkHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL cidr data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-net-types.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("cidr", EDBDbType.Cidr)]
    public class CidrHandler : EDBSimpleTypeHandler<(IPAddress Address, int Subnet)>, IEDBSimpleTypeHandler<EDBInet>
    {
        /// <inheritdoc />
        public CidrHandler(PostgresType postgresType) : base(postgresType) {}

        /// <inheritdoc />
        public override (IPAddress Address, int Subnet) Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => InetHandler.DoRead(buf, len, fieldDescription, true);

        EDBInet IEDBSimpleTypeHandler<EDBInet>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        {
            var (address, subnet) = Read(buf, len, fieldDescription);
            return new EDBInet(address, subnet);
        }

        /// <inheritdoc />
        public override int ValidateAndGetLength((IPAddress Address, int Subnet) value, EDBParameter? parameter)
            => InetHandler.GetLength(value.Address);

        /// <inheritdoc />
        public int ValidateAndGetLength(EDBInet value, EDBParameter? parameter)
            => InetHandler.GetLength(value.Address);

        /// <inheritdoc />
        public override void Write((IPAddress Address, int Subnet) value, EDBWriteBuffer buf, EDBParameter? parameter)
            => InetHandler.DoWrite(value.Address, value.Subnet, buf, true);

        /// <inheritdoc />
        public void Write(EDBInet value, EDBWriteBuffer buf, EDBParameter? parameter)
            => InetHandler.DoWrite(value.Address, value.Netmask, buf, true);
    }
}
