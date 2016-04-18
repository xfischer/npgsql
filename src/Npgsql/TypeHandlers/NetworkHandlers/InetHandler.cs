#region License
// The PostgreSQL License
//
// Copyright (C) 2015 The  EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using  EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;

namespace  EnterpriseDB.EDBClient.TypeHandlers.NetworkHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-net-types.html
    /// </remarks>
    [TypeMapping("inet", EDBDbType.Inet, new[] { typeof(EDBInet), typeof(IPAddress) })]
    internal class InetHandler : TypeHandlerWithPsv<IPAddress, EDBInet>,
        ISimpleTypeReader<IPAddress>, ISimpleTypeReader<EDBInet>, ISimpleTypeWriter,
        ISimpleTypeReader<string>
    {
        const byte IPv4 = 2;
        const byte IPv6 = 3;

        public IPAddress Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return ((ISimpleTypeReader<EDBInet>)this).Read(buf, len, fieldDescription).Address;
        }

        static internal EDBInet DoRead(EDBBuffer buf, FieldDescription fieldDescription, int len, bool isCidrHandler)
        {
            buf.ReadByte();  // addressFamily
            var mask = buf.ReadByte();
            var isCidr = buf.ReadByte() == 1;
            Contract.Assume(isCidrHandler == isCidr);
            var numBytes = buf.ReadByte();
            var bytes = new byte[numBytes];
            for (var i = 0; i < numBytes; i++) {
                bytes[i] = buf.ReadByte();
            }
            return new EDBInet(new IPAddress(bytes), mask);
        }

        EDBInet ISimpleTypeReader<EDBInet>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return DoRead(buf, fieldDescription, len, false);
        }

        string ISimpleTypeReader<string>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return ((ISimpleTypeReader<EDBInet>)this).Read(buf, len, fieldDescription).ToString();
        }

        static internal int DoValidateAndGetLength(object value)
        {
            IPAddress ip;
            if (value is EDBInet) {
                ip = ((EDBInet)value).Address;
            } else {
                ip = value as IPAddress;
                if (ip == null) {
                    throw new InvalidCastException(String.Format("Can't send type {0} as inet", value.GetType()));
                }
            }

            switch (ip.AddressFamily) {
            case AddressFamily.InterNetwork:
                return 8;
            case AddressFamily.InterNetworkV6:
                return 20;
            default:
                throw new InvalidCastException(String.Format("Can't handle IPAddress with AddressFamily {0}, only InterNetwork or InterNetworkV6!", ip.AddressFamily));
            }
        }

        public int ValidateAndGetLength(object value, EDBParameter parameter)
        {
            return DoValidateAndGetLength(value);
        }

        internal static void DoWrite(object value, EDBBuffer buf, bool isCidrHandler)
        {
            IPAddress ip;
            int mask;
            if (value is EDBInet) {
                var inet = ((EDBInet)value);
                ip = inet.Address;
                mask = inet.Netmask;
            } else {
                ip = value as IPAddress;
                if (ip == null) {
                    throw new InvalidCastException(String.Format("Can't send type {0} as inet", value.GetType()));
                }
                mask = -1;
            }

            switch (ip.AddressFamily) {
            case AddressFamily.InterNetwork:
                buf.WriteByte(IPv4);
                if (mask == -1) {
                    mask = 32;
                }
                break;
            case AddressFamily.InterNetworkV6:
                buf.WriteByte(IPv6);
                if (mask == -1) {
                    mask = 128;
                }
                break;
            default:
                throw new InvalidCastException(String.Format("Can't handle IPAddress with AddressFamily {0}, only InterNetwork or InterNetworkV6!", ip.AddressFamily));
            }

            buf.WriteByte((byte)mask);
            buf.WriteByte((byte)(isCidrHandler ? 1 : 0));  // Ignored on server side
            var bytes = ip.GetAddressBytes();
            buf.WriteByte((byte)bytes.Length);
            buf.WriteBytes(bytes, 0, bytes.Length);
        }

        public void Write(object value, EDBBuffer buf, EDBParameter parameter)
        {
            DoWrite(value, buf, false);
        }
    }
}
