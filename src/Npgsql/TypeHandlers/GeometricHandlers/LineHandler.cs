using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.GeometricHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL line data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-geometric.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("line", EDBDbType.Line, typeof(EDBLine))]
    public class LineHandler : EDBSimpleTypeHandler<EDBLine>
    {
        /// <inheritdoc />
        public LineHandler(PostgresType postgresType) : base(postgresType) {}

        /// <inheritdoc />
        public override EDBLine Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => new EDBLine(buf.ReadDouble(), buf.ReadDouble(), buf.ReadDouble());

        /// <inheritdoc />
        public override int ValidateAndGetLength(EDBLine value, EDBParameter? parameter)
            => 24;

        /// <inheritdoc />
        public override void Write(EDBLine value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            buf.WriteDouble(value.A);
            buf.WriteDouble(value.B);
            buf.WriteDouble(value.C);
        }
    }
}
