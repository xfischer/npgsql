using System;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.GeometricHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL path data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-geometric.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("path", EDBDbType.Path, typeof(EDBPath))]
    public class PathHandler : EDBTypeHandler<EDBPath>
    {
        /// <inheritdoc />
        public PathHandler(PostgresType postgresType) : base(postgresType) {}

        #region Read

        /// <inheritdoc />
        public override async ValueTask<EDBPath> Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
        {
            await buf.Ensure(5, async);
            var open = buf.ReadByte() switch
            {
                1 => false,
                0 => true,
                _ => throw new Exception("Error decoding binary geometric path: bad open byte")
            };

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

        /// <inheritdoc />
        public override int ValidateAndGetLength(EDBPath value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => 5 + value.Count * 16;

        /// <inheritdoc />
        public override async Task Write(EDBPath value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async)
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
