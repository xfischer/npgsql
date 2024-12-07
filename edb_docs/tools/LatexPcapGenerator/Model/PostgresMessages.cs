namespace pcap2latex;

public record struct PostgresMessage(char code, string Name);

public static class PostgresMessages
{
    public static PostgresMessage Undefined => new PostgresMessage('0', "Undefined");
    
    private static readonly Dictionary<char, PostgresMessage> backendMessages = new();
    private static readonly Dictionary<char, PostgresMessage> frontendMessages = new();

    public static PostgresMessage? GetMessage(char messageCode, bool? frontEnd)
    {
        if (frontEnd ?? false)
        {
            if (frontendMessages.TryGetValue(messageCode, out var message))
                return message;
        }
        else
        {
            if (backendMessages.TryGetValue(messageCode, out var message))
                return message;
        }
        return null;
    }

    static PostgresMessages()
    {
        AddBackendMessage(new PostgresMessage('R', "AuthenticationRequest"));
        AddBackendMessage(new PostgresMessage('K', "BackendKeyData"));
        AddBackendMessage(new PostgresMessage('2', "BindComplete"));
        AddBackendMessage(new PostgresMessage('3', "CloseComplete"));
        AddBackendMessage(new PostgresMessage('C', "CommandComplete"));
        AddBackendMessage(new PostgresMessage('d', "CopyData"));
        AddBackendMessage(new PostgresMessage('c', "CopyDone"));
        AddBackendMessage(new PostgresMessage('W', "CopyBothResponse"));
        AddBackendMessage(new PostgresMessage('G', "CopyInResponse"));
        AddBackendMessage(new PostgresMessage('H', "CopyOutResponse"));
        AddBackendMessage(new PostgresMessage('D', "DataRow"));
        AddBackendMessage(new PostgresMessage('I', "EmptyQueryResponse"));
        AddBackendMessage(new PostgresMessage('E', "ErrorResponse"));
        AddBackendMessage(new PostgresMessage('F', "FunctionCall"));
        AddBackendMessage(new PostgresMessage('V', "FunctionCallResponse"));
        AddBackendMessage(new PostgresMessage('n', "NoData"));
        AddBackendMessage(new PostgresMessage('N', "NoticeResponse"));
        AddBackendMessage(new PostgresMessage('A', "NotificationResponse"));
        AddBackendMessage(new PostgresMessage('t', "ParameterDescription"));
        AddBackendMessage(new PostgresMessage('S', "ParameterStatus"));
        AddBackendMessage(new PostgresMessage('1', "ParseComplete"));
        AddBackendMessage(new PostgresMessage(' ', "PasswordPacket"));
        AddBackendMessage(new PostgresMessage('s', "PortalSuspended"));
        AddBackendMessage(new PostgresMessage('Z', "ReadyForQuery"));
        AddBackendMessage(new PostgresMessage('T', "RowDescription"));
        AddBackendMessage(new PostgresMessage('u', "OutDescription"));
        AddBackendMessage(new PostgresMessage('v', "ParamData"));
        AddBackendMessage(new PostgresMessage('?', "StartupMessage"));

        AddFrontendMessage(new PostgresMessage('?', "StartupMessage"));
        AddFrontendMessage(new PostgresMessage('D', "Describe"));
        AddFrontendMessage(new PostgresMessage('S', "Sync"));
        AddFrontendMessage(new PostgresMessage('E', "Execute"));
        AddFrontendMessage(new PostgresMessage('P', "Parse"));
        AddFrontendMessage(new PostgresMessage('B', "Bind"));
        AddFrontendMessage(new PostgresMessage('C', "Close"));
        AddFrontendMessage(new PostgresMessage('Q', "Query"));
        AddFrontendMessage(new PostgresMessage('d', "CopyData"));
        AddFrontendMessage(new PostgresMessage('c', "CopyDone"));
        AddFrontendMessage(new PostgresMessage('f', "CopyFail"));
        AddFrontendMessage(new PostgresMessage('X', "Terminate"));
        AddFrontendMessage(new PostgresMessage('p', "Password"));
        AddFrontendMessage(new PostgresMessage('O', "ParseOut"));
        AddFrontendMessage(new PostgresMessage('u', "DescribeOut"));
        AddFrontendMessage(new PostgresMessage('v', "ExecuteOut"));
    }

    private static void AddBackendMessage(PostgresMessage message) => backendMessages.Add(message.code, message);
    private static void AddFrontendMessage(PostgresMessage message) => frontendMessages.Add(message.code, message);

}
