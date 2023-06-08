using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.NumericHandlers;

/// <summary>
/// A type handler for PostgreSQL unsigned 32-bit data types. This is only used for internal types.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/datatype-oid.html.
///
/// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
/// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
/// Use it at your own risk.
/// </remarks>
public partial class UInt32Handler : EDBSimpleTypeHandler<uint>
{
    public UInt32Handler(PostgresType pgType) : base(pgType) {}

    /// <inheritdoc />
    public override uint Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        => buf.ReadUInt32();

    /// <inheritdoc />
    public override int ValidateAndGetLength(uint value, EDBParameter? parameter) => 4;

    /// <inheritdoc />
    public override void Write(uint value, EDBWriteBuffer buf, EDBParameter? parameter)
        => buf.WriteUInt32(value);
}