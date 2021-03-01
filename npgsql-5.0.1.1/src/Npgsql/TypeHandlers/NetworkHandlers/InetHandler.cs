using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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
    /// See https://www.postgresql.org/docs/current/static/datatype-net-types.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping(
        "inet",
        EDBDbType.Inet,
        new[] { typeof(IPAddress), typeof((IPAddress Address, int Subnet)), typeof(EDBInet) })]
    public class InetHandler : EDBSimpleTypeHandlerWithPsv<IPAddress, (IPAddress Address, int Subnet)>,
        IEDBSimpleTypeHandler<EDBInet>
    {
        // ReSharper disable InconsistentNaming
        const byte IPv4 = 2;
        const byte IPv6 = 3;
        // ReSharper restore InconsistentNaming

        /// <inheritdoc />
        public InetHandler(PostgresType postgresType) : base(postgresType) {}

        #region Read

        /// <inheritdoc />
        public override IPAddress Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => DoRead(buf, len, fieldDescription, false).Address;

#pragma warning disable CA1801 // Review unused parameters
        internal static (IPAddress Address, int Subnet) DoRead(
            EDBReadBuffer buf,
            int len,
            FieldDescription? fieldDescription,
            bool isCidrHandler)
        {
            buf.ReadByte(); // addressFamily
            var mask = buf.ReadByte();
            var isCidr = buf.ReadByte() == 1;
            Debug.Assert(isCidrHandler == isCidr);
            var numBytes = buf.ReadByte();
            var bytes = new byte[numBytes];
            for (var i = 0; i < numBytes; i++)
                bytes[i] = buf.ReadByte();

            return (new IPAddress(bytes), mask);
        }
#pragma warning restore CA1801 // Review unused parameters

        /// <inheritdoc />
        protected override (IPAddress Address, int Subnet) ReadPsv(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => DoRead(buf, len, fieldDescription, false);

        EDBInet IEDBSimpleTypeHandler<EDBInet>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        {
            var (address, subnet) = DoRead(buf, len, fieldDescription, false);
            return new EDBInet(address, subnet);
        }

        #endregion Read

        #region Write

        /// <inheritdoc />
        protected internal override int ValidateObjectAndGetLength(object value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => value switch {
                null => -1,
                DBNull _ => -1,
                IPAddress ip => ValidateAndGetLength(ip, parameter),
                ValueTuple<IPAddress, int> tup => ValidateAndGetLength(tup, parameter),
                EDBInet inet => ValidateAndGetLength(inet, parameter),
                _ => throw new InvalidCastException($"Can't write CLR type {value.GetType().Name} to database type {PgDisplayName}")
            };

        /// <inheritdoc />
        protected internal override Task WriteObjectWithLength(object value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => value switch {
                DBNull _ => WriteWithLengthInternal(DBNull.Value, buf, lengthCache, parameter, async, cancellationToken),
                IPAddress ip => WriteWithLengthInternal(ip, buf, lengthCache, parameter, async, cancellationToken),
                ValueTuple<IPAddress, int> tup => WriteWithLengthInternal(tup, buf, lengthCache, parameter, async, cancellationToken),
                EDBInet inet => WriteWithLengthInternal(inet, buf, lengthCache, parameter, async, cancellationToken),
                _ => throw new InvalidCastException($"Can't write CLR type {value.GetType().Name} to database type {PgDisplayName}")
            };

        /// <inheritdoc />
        public override int ValidateAndGetLength(IPAddress value, EDBParameter? parameter)
            => GetLength(value);

        /// <inheritdoc />
        public override int ValidateAndGetLength((IPAddress Address, int Subnet) value, EDBParameter? parameter)
            => GetLength(value.Address);

        /// <inheritdoc />
        public int ValidateAndGetLength(EDBInet value, EDBParameter? parameter)
            => GetLength(value.Address);

        /// <inheritdoc />
        public override void Write(IPAddress value, EDBWriteBuffer buf, EDBParameter? parameter)
            => DoWrite(value, -1, buf, false);

        /// <inheritdoc />
        public override void Write((IPAddress Address, int Subnet) value, EDBWriteBuffer buf, EDBParameter? parameter)
            => DoWrite(value.Address, value.Subnet, buf, false);

        /// <inheritdoc />
        public void Write(EDBInet value, EDBWriteBuffer buf, EDBParameter? parameter)
            => DoWrite(value.Address, value.Netmask, buf, false);

        internal static void DoWrite(IPAddress ip, int mask, EDBWriteBuffer buf, bool isCidrHandler)
        {
            switch (ip.AddressFamily) {
            case AddressFamily.InterNetwork:
                buf.WriteByte(IPv4);
                if (mask == -1)
                    mask = 32;
                break;
            case AddressFamily.InterNetworkV6:
                buf.WriteByte(IPv6);
                if (mask == -1)
                    mask = 128;
                break;
            default:
                throw new InvalidCastException($"Can't handle IPAddress with AddressFamily {ip.AddressFamily}, only InterNetwork or InterNetworkV6!");
            }

            buf.WriteByte((byte)mask);
            buf.WriteByte((byte)(isCidrHandler ? 1 : 0));  // Ignored on server side
            var bytes = ip.GetAddressBytes();
            buf.WriteByte((byte)bytes.Length);
            buf.WriteBytes(bytes, 0, bytes.Length);
        }

        internal static int GetLength(IPAddress value)
            => value.AddressFamily switch
            {
                AddressFamily.InterNetwork   => 8,
                AddressFamily.InterNetworkV6 => 20,
                _ => throw new InvalidCastException($"Can't handle IPAddress with AddressFamily {value.AddressFamily}, only InterNetwork or InterNetworkV6!")
            };

        #endregion Write
    }
}
