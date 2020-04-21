using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.GeometricHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL lseg data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-geometric.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("lseg", EDBDbType.LSeg, typeof(EDBLSeg))]
    public class LineSegmentHandler : EDBSimpleTypeHandler<EDBLSeg>
    {
        /// <inheritdoc />
        public LineSegmentHandler(PostgresType postgresType) : base(postgresType) {}

        /// <inheritdoc />
        public override EDBLSeg Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => new EDBLSeg(buf.ReadDouble(), buf.ReadDouble(), buf.ReadDouble(), buf.ReadDouble());

        /// <inheritdoc />
        public override int ValidateAndGetLength(EDBLSeg value, EDBParameter? parameter)
            => 32;

        /// <inheritdoc />
        public override void Write(EDBLSeg value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            buf.WriteDouble(value.Start.X);
            buf.WriteDouble(value.Start.Y);
            buf.WriteDouble(value.End.X);
            buf.WriteDouble(value.End.Y);
        }
    }
}
