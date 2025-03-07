using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace pcap2latex;



public sealed class PcapPostgresOptions
{
    /// <summary>
    /// Delegate called to when no conversion was found for a given <see cref="PostgresMessage"/>.
    /// Implementers should return an <see cref="IPostgresMessage"/> instance or <c>null</c> when message can't be parsed
    /// </summary>
    public Func<PostgresMessage, ParserInfo, IPostgresMessage?>? CustomMessageProcessor;

    /// <summary>
    /// Delegate called to provide additionnal template for a given <see cref="IPostgresMessage"/>.
    /// Implementers should return an <see cref="ITextTransformer"/> instance or <c>null</c> when message should be transformed using default transformer
    /// </summary>
    /// <remarks>Any template returned by this function will take precedence over the default template.</remarks>
    public Func<IPostgresMessage, ITextTransformer?>? CustomTemplateProvider;

    public Func<string?, GenerationState, ITextTransformer?>? CustomHeaderProvider;

    public IPostgresMessageRegistry MessageCatalog { get; set; } = new PostgresMessageRegistry();
}

internal static class PcapPostgresOptionsExtensions
{
    public static void AddDefaultPostgresMessages(this PcapPostgresOptions options)
    {
        options.MessageCatalog.AddOrReplaceBackendMessage(new('R', "AuthenticationRequest"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('K', "BackendKeyData"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('2', "BindComplete"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('3', "CloseComplete"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('C', "CommandComplete"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('d', "CopyData"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('c', "CopyDone"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('W', "CopyBothResponse"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('G', "CopyInResponse"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('H', "CopyOutResponse"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('D', "DataRow"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('I', "EmptyQueryResponse"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('E', "ErrorResponse"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('F', "FunctionCall"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('V', "FunctionCallResponse"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('n', "NoData"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('N', "NoticeResponse"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('A', "NotificationResponse"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('t', "ParameterDescription"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('S', "ParameterStatus"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('1', "ParseComplete"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new(' ', "PasswordPacket"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('s', "PortalSuspended"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('Z', "ReadyForQuery"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('T', "RowDescription"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('v', "NegotiateProtocolVersion"));

        options.MessageCatalog.AddOrReplaceBackendMessage(new('?', "StartupMessage"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('?', "StartupMessage"));

        options.MessageCatalog.AddOrReplaceFrontendMessage(new('D', "Describe"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('S', "Sync"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('E', "Execute"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('P', "Parse"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('B', "Bind"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('C', "Close"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('Q', "Query"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('d', "CopyData"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('c', "CopyDone"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('f', "CopyFail"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('X', "Terminate"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('p', "Password"));
    }
        
}