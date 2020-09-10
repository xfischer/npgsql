using System.Data;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL smallint data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-numeric.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("smallint", EDBDbType.Smallint, new[] { DbType.Int16, DbType.Byte, DbType.SByte }, new[] { typeof(short), typeof(byte), typeof(sbyte) }, DbType.Int16)]
    public class Int16Handler : EDBSimpleTypeHandler<short>,
        IEDBSimpleTypeHandler<byte>, IEDBSimpleTypeHandler<sbyte>, IEDBSimpleTypeHandler<int>, IEDBSimpleTypeHandler<long>,
        IEDBSimpleTypeHandler<float>, IEDBSimpleTypeHandler<double>, IEDBSimpleTypeHandler<decimal>
    {
        /// <inheritdoc />
        public Int16Handler(PostgresType postgresType) : base(postgresType) {}

        #region Read

        /// <inheritdoc />
        public override short Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => buf.ReadInt16();

        byte IEDBSimpleTypeHandler<byte>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => checked((byte)Read(buf, len, fieldDescription));

        sbyte IEDBSimpleTypeHandler<sbyte>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => checked((sbyte)Read(buf, len, fieldDescription));

        int IEDBSimpleTypeHandler<int>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => Read(buf, len, fieldDescription);

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
        public override int ValidateAndGetLength(short value, EDBParameter? parameter) => 2;
        /// <inheritdoc />
        public int ValidateAndGetLength(int value, EDBParameter? parameter)            => 2;
        /// <inheritdoc />
        public int ValidateAndGetLength(long value, EDBParameter? parameter)           => 2;
        /// <inheritdoc />
        public int ValidateAndGetLength(byte value, EDBParameter? parameter)           => 2;
        /// <inheritdoc />
        public int ValidateAndGetLength(sbyte value, EDBParameter? parameter)          => 2;
        /// <inheritdoc />
        public int ValidateAndGetLength(float value, EDBParameter? parameter)          => 2;
        /// <inheritdoc />
        public int ValidateAndGetLength(double value, EDBParameter? parameter)         => 2;
        /// <inheritdoc />
        public int ValidateAndGetLength(decimal value, EDBParameter? parameter)        => 2;

        /// <inheritdoc />
        public override void Write(short value, EDBWriteBuffer buf, EDBParameter? parameter) => buf.WriteInt16(value);
        /// <inheritdoc />
        public void Write(int value, EDBWriteBuffer buf, EDBParameter? parameter)            => buf.WriteInt16(checked((short)value));
        /// <inheritdoc />
        public void Write(long value, EDBWriteBuffer buf, EDBParameter? parameter)           => buf.WriteInt16(checked((short)value));
        /// <inheritdoc />
        public void Write(byte value, EDBWriteBuffer buf, EDBParameter? parameter)           => buf.WriteInt16(value);
        /// <inheritdoc />
        public void Write(sbyte value, EDBWriteBuffer buf, EDBParameter? parameter)          => buf.WriteInt16(value);
        /// <inheritdoc />
        public void Write(decimal value, EDBWriteBuffer buf, EDBParameter? parameter)        => buf.WriteInt16((short)value);
        /// <inheritdoc />
        public void Write(double value, EDBWriteBuffer buf, EDBParameter? parameter)         => buf.WriteInt16(checked((short)value));
        /// <inheritdoc />
        public void Write(float value, EDBWriteBuffer buf, EDBParameter? parameter)          => buf.WriteInt16(checked((short)value));

        #endregion Write
    }
}
