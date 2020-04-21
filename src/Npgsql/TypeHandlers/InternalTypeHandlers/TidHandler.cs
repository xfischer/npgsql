using System.Diagnostics;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.InternalTypeHandlers
{
    [TypeMapping("tid", EDBDbType.Tid, typeof(EDBTid))]
    class TidHandler : EDBSimpleTypeHandler<EDBTid>
    {
        public TidHandler(PostgresType postgresType) : base(postgresType) {}

        #region Read

        public override EDBTid Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            Debug.Assert(len == 6);

            var blockNumber = buf.ReadUInt32();
            var offsetNumber = buf.ReadUInt16();

            return new EDBTid(blockNumber, offsetNumber);
        }

        #endregion Read

        #region Write

        public override int ValidateAndGetLength(EDBTid value, EDBParameter? parameter)
            => 6;

        public override void Write(EDBTid value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            buf.WriteUInt32(value.BlockNumber);
            buf.WriteUInt16(value.OffsetNumber);
        }

        #endregion Write
    }
}
