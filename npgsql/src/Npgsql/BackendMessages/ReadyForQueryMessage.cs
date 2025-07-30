using EnterpriseDB.EDBClient.Internal;

namespace EnterpriseDB.EDBClient.BackendMessages;

sealed class ReadyForQueryMessage : IBackendMessage
{
    public BackendMessageCode Code => BackendMessageCode.ReadyForQuery;

    internal TransactionStatus TransactionStatusIndicator { get; private set; }

    internal ReadyForQueryMessage Load(EDBReadBuffer buf) {
        TransactionStatusIndicator = (TransactionStatus)buf.ReadByte();
        return this;
    }
}