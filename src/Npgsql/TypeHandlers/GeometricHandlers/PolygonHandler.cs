#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The  EnterpriseDB.EDBClient DEVELOPMENT Team
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

using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

namespace  EnterpriseDB.EDBClient.TypeHandlers.GeometricHandlers
{
    /// <summary>
    /// Type handler for the PostgreSQL geometric polygon type.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-geometric.html
    /// </remarks>
    [TypeMapping("polygon", EDBDbType.Polygon, typeof(EDBPolygon))]
    class PolygonHandler : ChunkingTypeHandler<EDBPolygon>
    {
        internal PolygonHandler(PostgresType postgresType) : base(postgresType) { }

        #region Read

        public override async ValueTask<EDBPolygon> Read(ReadBuffer buf, int len, bool async, FieldDescription fieldDescription = null)
        {
            await buf.Ensure(4, async);
            var numPoints = buf.ReadInt32();
            var result = new EDBPolygon(numPoints);
            for (var i = 0; i < numPoints; i++)
            {
                await buf.Ensure(16, async);
                result.Add(new EDBPoint(buf.ReadDouble(), buf.ReadDouble()));
            }
            return result;
        }

        #endregion

        #region Write

        public override int ValidateAndGetLength(object value, ref LengthCache lengthCache, EDBParameter parameter = null)
        {
            if (!(value is EDBPolygon))
                throw CreateConversionException(value.GetType());
            return 4 + ((EDBPolygon)value).Count * 16;
        }

        protected override async Task Write(object value, WriteBuffer buf, LengthCache lengthCache, EDBParameter parameter,
            bool async, CancellationToken cancellationToken)
        {
            var polygon = (EDBPolygon)value;

            if (buf.WriteSpaceLeft < 4)
                await buf.Flush(async, cancellationToken);
            buf.WriteInt32(polygon.Count);

            foreach (var p in polygon)
            {
                if (buf.WriteSpaceLeft < 16)
                    await buf.Flush(async, cancellationToken);
                buf.WriteDouble(p.X);
                buf.WriteDouble(p.Y);
            }
        }

        #endregion
    }
}
