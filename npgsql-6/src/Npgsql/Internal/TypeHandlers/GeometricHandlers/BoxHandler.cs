using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.GeometricHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL box data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-geometric.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    public partial class BoxHandler : EDBSimpleTypeHandler<EDBBox>
    {
        public BoxHandler(PostgresType pgType) : base(pgType) {}

        /// <inheritdoc />
        public override EDBBox Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => new(
                new EDBPoint(buf.ReadDouble(), buf.ReadDouble()),
                new EDBPoint(buf.ReadDouble(), buf.ReadDouble())
            );

        /// <inheritdoc />
        public override int ValidateAndGetLength(EDBBox value, EDBParameter? parameter)
            => 32;

        /// <inheritdoc />
        public override void Write(EDBBox value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            buf.WriteDouble(value.Right);
            buf.WriteDouble(value.Top);
            buf.WriteDouble(value.Left);
            buf.WriteDouble(value.Bottom);
        }
    }
}
