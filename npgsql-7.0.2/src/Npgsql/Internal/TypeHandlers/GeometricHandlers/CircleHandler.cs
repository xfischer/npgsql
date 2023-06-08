using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.GeometricHandlers;

/// <summary>
/// A type handler for the PostgreSQL circle data type.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/datatype-geometric.html.
///
/// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
/// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
/// Use it at your own risk.
/// </remarks>
public partial class CircleHandler : EDBSimpleTypeHandler<EDBCircle>
{
    public CircleHandler(PostgresType pgType) : base(pgType) {}

    /// <inheritdoc />
    public override EDBCircle Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        => new(buf.ReadDouble(), buf.ReadDouble(), buf.ReadDouble());

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