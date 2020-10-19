using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.GeometricHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL circle data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-geometric.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("circle", EDBDbType.Circle, typeof(EDBCircle))]
    public class CircleHandler : EDBSimpleTypeHandler<EDBCircle>
    {
        /// <inheritdoc />
        public CircleHandler(PostgresType postgresType) : base(postgresType) {}

        /// <inheritdoc />
        public override EDBCircle Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => new EDBCircle(buf.ReadDouble(), buf.ReadDouble(), buf.ReadDouble());

        /// <inheritdoc />
        public override int ValidateAndGetLength(EDBCircle value, EDBParameter? parameter)
            => 24;

        /// <inheritdoc />
        public override void Write(EDBCircle value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            buf.WriteDouble(value.X);
            buf.WriteDouble(value.Y);
            buf.WriteDouble(value.Radius);
        }
    }
}
