#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EDB Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EDB DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EDB DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EDB DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EDB DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.GeometricHandlers
{
    /// <summary>
    /// Type handler for the PostgreSQL geometric path segment type (open or closed).
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-geometric.html
    /// </remarks>
    [TypeMapping("path", EDBDbType.Path, typeof(EDBPath))]
    class PathHandler : EDBTypeHandler<EDBPath>
    {
        #region Read

        public override async ValueTask<EDBPath> Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription = null)
        {
            await buf.Ensure(5, async);
            bool open;
            var openByte = buf.ReadByte();
            switch (openByte)
            {
            case 1:
                open = false;
                break;
            case 0:
                open = true;
                break;
            default:
                throw new Exception("Error decoding binary geometric path: bad open byte");
            }

            var numPoints = buf.ReadInt32();
            var result = new EDBPath(numPoints, open);
            for (var i = 0; i < numPoints; i++)
            {
                await buf.Ensure(16, async);
                result.Add(new EDBPoint(buf.ReadDouble(), buf.ReadDouble()));
            }
            return result;
        }

        #endregion

        #region Write

        public override int ValidateAndGetLength(EDBPath value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => 5 + value.Count * 16;

        public override async Task Write(EDBPath value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
        {
            if (buf.WriteSpaceLeft < 5)
                await buf.Flush(async);
            buf.WriteByte((byte)(value.Open ? 0 : 1));
            buf.WriteInt32(value.Count);

            foreach (var p in value)
            {
                if (buf.WriteSpaceLeft < 16)
                    await buf.Flush(async);
                buf.WriteDouble(p.X);
                buf.WriteDouble(p.Y);
            }
        }

        #endregion
    }
}
