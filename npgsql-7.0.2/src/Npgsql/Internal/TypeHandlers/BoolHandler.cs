using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers;

/// <summary>
/// A type handler for the PostgreSQL bool data type.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/datatype-boolean.html.
///
/// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
/// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
/// Use it at your own risk.
/// </remarks>
public partial class BoolHandler : EDBSimpleTypeHandler<bool>
{
    public BoolHandler(PostgresType pgType) : base(pgType) {}

    /// <inheritdoc />
    public override bool Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        => buf.ReadByte() != 0;

    /// <inheritdoc />
    public override int ValidateAndGetLength(bool value, EDBParameter? parameter)
        => 1;

    /// <inheritdoc />
    public override void Write(bool value, EDBWriteBuffer buf, EDBParameter? parameter)
        => buf.WriteByte(value ? (byte)1 : (byte)0);
}