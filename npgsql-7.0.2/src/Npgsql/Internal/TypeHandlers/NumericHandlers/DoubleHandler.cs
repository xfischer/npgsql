using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.NumericHandlers;

/// <summary>
/// A type handler for the PostgreSQL double precision data type.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/datatype-numeric.html.
///
/// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
/// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
/// Use it at your own risk.
/// </remarks>
public partial class DoubleHandler : EDBSimpleTypeHandler<double>
{
    public DoubleHandler(PostgresType pgType) : base(pgType) {}

    /// <inheritdoc />
    public override double Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        => buf.ReadDouble();

    /// <inheritdoc />
    public override int ValidateAndGetLength(double value, EDBParameter? parameter)
        => 8;

    /// <inheritdoc />
    public override void Write(double value, EDBWriteBuffer buf, EDBParameter? parameter)
        => buf.WriteDouble(value);
}