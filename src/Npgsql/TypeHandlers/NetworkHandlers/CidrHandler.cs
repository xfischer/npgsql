#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System.Net;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

#pragma warning disable 618

namespace EnterpriseDB.EDBClient.TypeHandlers.NetworkHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-net-types.html
    /// </remarks>
    [TypeMapping("cidr", EDBDbType.Cidr)]
    class CidrHandler : EDBSimpleTypeHandler<(IPAddress Address, int Subnet)>, IEDBSimpleTypeHandler<EDBInet>
    {
        public override (IPAddress Address, int Subnet) Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
            => InetHandler.DoRead(buf, len, fieldDescription, true);

        EDBInet IEDBSimpleTypeHandler<EDBInet>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
        {
            var (address, subnet) = Read(buf, len, fieldDescription);
            return new EDBInet(address, subnet);
        }

        public override int ValidateAndGetLength((IPAddress Address, int Subnet) value, EDBParameter parameter)
            => InetHandler.GetLength(value.Address);

        public int ValidateAndGetLength(EDBInet value, EDBParameter parameter)
            => InetHandler.GetLength(value.Address);

        public override void Write((IPAddress Address, int Subnet) value, EDBWriteBuffer buf, EDBParameter parameter)
            => InetHandler.DoWrite(value.Address, value.Subnet, buf, true);

        public void Write(EDBInet value, EDBWriteBuffer buf, EDBParameter parameter)
            => InetHandler.DoWrite(value.Address, value.Netmask, buf, true);
    }
}
