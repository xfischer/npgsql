using System.Diagnostics;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.InternalTypeHandlers;

sealed partial class TidHandler : EDBSimpleTypeHandler<EDBTid>
{
    public TidHandler(PostgresType pgType) : base(pgType) {}

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