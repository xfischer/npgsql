using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace pcap2latex;



public sealed class PcapPostgresOptions
{
    /// <summary>
    /// Delegate called to when no conversion was found for a given <see cref="PostgresMessage"/>.
    /// Implementers should return an <see cref="PostgresMessageBase"/> instance or <c>null</c> when message can't be parsed
    /// </summary>
    public Func<PostgresMessage, ParserInfo, PostgresMessageBase?>? CustomMessageProcessor;

    public IPostgresMessageRegistry MessageCatalog { get; set; } = new PostgresMessageRegistry();
}

public static class PcapPostgresOptionsExtensions
{
    public static void AddDefaultPostgresMessages(this PcapPostgresOptions options)
    {
        options.MessageCatalog.AddOrReplaceBackendMessage(new('R', "AuthenticationRequest", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('K', "BackendKeyData", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('2', "BindComplete", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('3', "CloseComplete", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('C', "CommandComplete", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('d', "CopyData", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('c', "CopyDone", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('W', "CopyBothResponse", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('G', "CopyInResponse", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('H', "CopyOutResponse", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('D', "DataRow", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('I', "EmptyQueryResponse", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('E', "ErrorResponse", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('F', "FunctionCall", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('V', "FunctionCallResponse", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('n', "NoData", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('N', "NoticeResponse", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('A', "NotificationResponse", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('t', "ParameterDescription", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('S', "ParameterStatus", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('1', "ParseComplete", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new(' ', "PasswordPacket", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('s', "PortalSuspended", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('Z', "ReadyForQuery", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('T', "RowDescription", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('v', "NegotiateProtocolVersion", IsFrontEnd: false));

        options.MessageCatalog.AddOrReplaceBackendMessage(new('?', "StartupMessage", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('?', "StartupMessage", IsFrontEnd: true));

        options.MessageCatalog.AddOrReplaceFrontendMessage(new('D', "Describe", IsFrontEnd: true));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('S', "Sync", IsFrontEnd: true));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('E', "Execute", IsFrontEnd: true));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('P', "Parse", IsFrontEnd: true));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('B', "Bind", IsFrontEnd: true));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('C', "Close", IsFrontEnd: true));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('Q', "Query", IsFrontEnd: true));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('d', "CopyData", IsFrontEnd: true));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('c', "CopyDone", IsFrontEnd: true));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('f', "CopyFail", IsFrontEnd: true));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('X', "Terminate", IsFrontEnd: true));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('p', "Password", IsFrontEnd: true));
    }
        
}