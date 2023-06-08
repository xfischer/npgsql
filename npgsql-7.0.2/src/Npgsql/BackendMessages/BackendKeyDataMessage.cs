using EnterpriseDB.EDBClient.Internal;

namespace EnterpriseDB.EDBClient.BackendMessages;

sealed class BackendKeyDataMessage : IBackendMessage
{
    public BackendMessageCode Code => BackendMessageCode.BackendKeyData;

    internal int BackendProcessId { get; private set; }
    internal int BackendSecretKey { get; private set; }

    internal BackendKeyDataMessage(EDBReadBuffer buf)
    {
        BackendProcessId = buf.ReadInt32();
        BackendSecretKey = buf.ReadInt32();
    }
}