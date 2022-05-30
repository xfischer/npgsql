using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.NumericHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL integer data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-numeric.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    public partial class Int32Handler : EDBSimpleTypeHandler<int>, IEDBSimpleTypeHandler<int>,
        IEDBSimpleTypeHandler<byte>, IEDBSimpleTypeHandler<short>, IEDBSimpleTypeHandler<long>,
        IEDBSimpleTypeHandler<float>, IEDBSimpleTypeHandler<double>, IEDBSimpleTypeHandler<decimal>
    {
        public Int32Handler(PostgresType pgType) : base(pgType) {}

        #region Read

        public override int Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => buf.ReadInt32();

        byte IEDBSimpleTypeHandler<byte>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => checked((byte)Read(buf, len, fieldDescription));

        short IEDBSimpleTypeHandler<short>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => checked((short)Read(buf, len, fieldDescription));

        long IEDBSimpleTypeHandler<long>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => Read(buf, len, fieldDescription);

        float IEDBSimpleTypeHandler<float>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => Read(buf, len, fieldDescription);

        double IEDBSimpleTypeHandler<double>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => Read(buf, len, fieldDescription);

        decimal IEDBSimpleTypeHandler<decimal>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => Read(buf, len, fieldDescription);

        #endregion Read

        #region Write

        /// <inheritdoc />
        public override int ValidateAndGetLength(int value, EDBParameter? parameter) => 4;
        /// <inheritdoc />
        public int ValidateAndGetLength(short value, EDBParameter? parameter)        => 4;
        /// <inheritdoc />
        public int ValidateAndGetLength(byte value, EDBParameter? parameter)         => 4;
        /// <inheritdoc />
        public int ValidateAndGetLength(decimal value, EDBParameter? parameter)      => 4;

        /// <inheritdoc />
        public int ValidateAndGetLength(long value, EDBParameter? parameter)
        {
            _ = checked((int)value);
            return 4;
        }

        /// <inheritdoc />
        public int ValidateAndGetLength(float value, EDBParameter? parameter)
        {
            _ = checked((int)value);
            return 4;
        }

        /// <inheritdoc />
        public int ValidateAndGetLength(double value, EDBParameter? parameter)
        {
            _ = checked((int)value);
            return 4;
        }

        /// <inheritdoc />
        public override void Write(int value, EDBWriteBuffer buf, EDBParameter? parameter) => buf.WriteInt32(value);
        /// <inheritdoc />
        public void Write(short value, EDBWriteBuffer buf, EDBParameter? parameter)        => buf.WriteInt32(value);
        /// <inheritdoc />
        public void Write(long value, EDBWriteBuffer buf, EDBParameter? parameter)         => buf.WriteInt32((int)value);
        /// <inheritdoc />
        public void Write(byte value, EDBWriteBuffer buf, EDBParameter? parameter)         => buf.WriteInt32(value);
        /// <inheritdoc />
        public void Write(float value, EDBWriteBuffer buf, EDBParameter? parameter)        => buf.WriteInt32((int)value);
        /// <inheritdoc />
        public void Write(double value, EDBWriteBuffer buf, EDBParameter? parameter)       => buf.WriteInt32((int)value);
        /// <inheritdoc />
        public void Write(decimal value, EDBWriteBuffer buf, EDBParameter? parameter)      => buf.WriteInt32((int)value);

        #endregion Write
    }
}
