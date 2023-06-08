using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.NumericHandlers;

/// <summary>
/// A type handler for the PostgreSQL bigint data type.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/datatype-numeric.html.
///
/// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
/// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
/// Use it at your own risk.
/// </remarks>
public partial class Int64Handler : EDBSimpleTypeHandler<long>,
    IEDBSimpleTypeHandler<byte>, IEDBSimpleTypeHandler<short>, IEDBSimpleTypeHandler<int>,
    IEDBSimpleTypeHandler<float>, IEDBSimpleTypeHandler<double>, IEDBSimpleTypeHandler<decimal>
{
    public Int64Handler(PostgresType pgType) : base(pgType) {}

    #region Read

    /// <inheritdoc />
    public override long Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        => buf.ReadInt64();

    byte IEDBSimpleTypeHandler<byte>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => checked((byte)Read(buf, len, fieldDescription));

    short IEDBSimpleTypeHandler<short>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => checked((short)Read(buf, len, fieldDescription));

    int IEDBSimpleTypeHandler<int>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => checked((int)Read(buf, len, fieldDescription));

    float IEDBSimpleTypeHandler<float>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => Read(buf, len, fieldDescription);

    double IEDBSimpleTypeHandler<double>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => Read(buf, len, fieldDescription);

    decimal IEDBSimpleTypeHandler<decimal>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => Read(buf, len, fieldDescription);

    #endregion Read

    #region Write

    /// <inheritdoc />
    public override int ValidateAndGetLength(long value, EDBParameter? parameter) => 8;
    /// <inheritdoc />
    public int ValidateAndGetLength(int value, EDBParameter? parameter)           => 8;
    /// <inheritdoc />
    public int ValidateAndGetLength(short value, EDBParameter? parameter)         => 8;
    /// <inheritdoc />
    public int ValidateAndGetLength(byte value, EDBParameter? parameter)          => 8;
    /// <inheritdoc />
    public int ValidateAndGetLength(decimal value, EDBParameter? parameter)       => 8;

    /// <inheritdoc />
    public int ValidateAndGetLength(float value, EDBParameter? parameter)
    {
        _ = checked((long)value);
        return 8;
    }

    /// <inheritdoc />
    public int ValidateAndGetLength(double value, EDBParameter? parameter)
    {
        _ = checked((long)value);
        return 8;
    }

    /// <inheritdoc />
    public override void Write(long value, EDBWriteBuffer buf, EDBParameter? parameter) => buf.WriteInt64(value);
    /// <inheritdoc />
    public void Write(short value, EDBWriteBuffer buf, EDBParameter? parameter)         => buf.WriteInt64(value);
    /// <inheritdoc />
    public void Write(int value, EDBWriteBuffer buf, EDBParameter? parameter)           => buf.WriteInt64(value);
    /// <inheritdoc />
    public void Write(byte value, EDBWriteBuffer buf, EDBParameter? parameter)          => buf.WriteInt64(value);
    /// <inheritdoc />
    public void Write(float value, EDBWriteBuffer buf, EDBParameter? parameter)         => buf.WriteInt64((long)value);
    /// <inheritdoc />
    public void Write(double value, EDBWriteBuffer buf, EDBParameter? parameter)        => buf.WriteInt64((long)value);
    /// <inheritdoc />
    public void Write(decimal value, EDBWriteBuffer buf, EDBParameter? parameter)       => buf.WriteInt64((long)value);

    #endregion Write
}