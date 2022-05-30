using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.InternalTypeHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL "char" type, used only internally.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-character.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    public partial class InternalCharHandler : EDBSimpleTypeHandler<char>,
        IEDBSimpleTypeHandler<byte>, IEDBSimpleTypeHandler<short>, IEDBSimpleTypeHandler<int>, IEDBSimpleTypeHandler<long>
    {
        public InternalCharHandler(PostgresType pgType) : base(pgType) {}

        #region Read

        /// <inheritdoc />
        public override char Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => (char)buf.ReadByte();

        byte IEDBSimpleTypeHandler<byte>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => buf.ReadByte();

        short IEDBSimpleTypeHandler<short>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => buf.ReadByte();

        int IEDBSimpleTypeHandler<int>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => buf.ReadByte();

        long IEDBSimpleTypeHandler<long>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => buf.ReadByte();

        #endregion

        #region Write

        /// <inheritdoc />
        public int ValidateAndGetLength(byte value, EDBParameter? parameter)          => 1;

        /// <inheritdoc />
        public override int ValidateAndGetLength(char value, EDBParameter? parameter)
        {
            _ = checked((byte)value);
            return 1;
        }

        /// <inheritdoc />
        public int ValidateAndGetLength(short value, EDBParameter? parameter)
        {
            _ = checked((byte)value);
            return 1;
        }

        /// <inheritdoc />
        public int ValidateAndGetLength(int value, EDBParameter? parameter)
        {
            _ = checked((byte)value);
            return 1;
        }

        /// <inheritdoc />
        public int ValidateAndGetLength(long value, EDBParameter? parameter)
        {
            _ = checked((byte)value);
            return 1;
        }

        /// <inheritdoc />
        public override void Write(char value, EDBWriteBuffer buf, EDBParameter? parameter) => buf.WriteByte((byte)value);
        /// <inheritdoc />
        public void Write(byte value, EDBWriteBuffer buf, EDBParameter? parameter) => buf.WriteByte(value);
        /// <inheritdoc />
        public void Write(short value, EDBWriteBuffer buf, EDBParameter? parameter) => buf.WriteByte((byte)value);
        /// <inheritdoc />
        public void Write(int value, EDBWriteBuffer buf, EDBParameter? parameter) => buf.WriteByte((byte)value);
        /// <inheritdoc />
        public void Write(long value, EDBWriteBuffer buf, EDBParameter? parameter) => buf.WriteByte((byte)value);

        #endregion
    }
}
