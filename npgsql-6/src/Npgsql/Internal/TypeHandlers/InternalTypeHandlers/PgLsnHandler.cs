using System.Diagnostics;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.InternalTypeHandlers
{
    partial class PgLsnHandler : EDBSimpleTypeHandler<EDBLogSequenceNumber>
    {
        public PgLsnHandler(PostgresType pgType) : base(pgType) {}

        #region Read

        public override EDBLogSequenceNumber Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            Debug.Assert(len == 8);
            return new EDBLogSequenceNumber(buf.ReadUInt64());
        }

        #endregion Read

        #region Write

        public override int ValidateAndGetLength(EDBLogSequenceNumber value, EDBParameter? parameter) => 8;

        public override void Write(EDBLogSequenceNumber value, EDBWriteBuffer buf, EDBParameter? parameter)
            => buf.WriteUInt64((ulong)value);

        #endregion Write
    }
}
