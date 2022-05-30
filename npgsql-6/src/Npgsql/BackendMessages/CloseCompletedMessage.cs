namespace EnterpriseDB.EDBClient.BackendMessages
{
    class CloseCompletedMessage : IBackendMessage
    {
        public BackendMessageCode Code => BackendMessageCode.CloseComplete;
        internal static readonly CloseCompletedMessage Instance = new();
        CloseCompletedMessage() { }
    }
}
