using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.GeometricHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL point data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-geometric.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("point", EDBDbType.Point, typeof(EDBPoint))]
    public class PointHandler : EDBSimpleTypeHandler<EDBPoint>
    {
        /// <inheritdoc />
        public PointHandler(PostgresType postgresType) : base(postgresType) {}

        /// <inheritdoc />
        public override EDBPoint Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => new EDBPoint(buf.ReadDouble(), buf.ReadDouble());

        /// <inheritdoc />
        public override int ValidateAndGetLength(EDBPoint value, EDBParameter? parameter)
            => 16;

        /// <inheritdoc />
        public override void Write(EDBPoint value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            buf.WriteDouble(value.X);
            buf.WriteDouble(value.Y);
        }
    }
}
