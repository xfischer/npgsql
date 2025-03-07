using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using pcap2latex;

namespace epascap2latex;

public static class EDBServiceCollectionExtensions
{
    public static IServiceCollection AddPcap2Latex_EPAS(this IServiceCollection services)
    {
        return services.AddPcap2Latex(options =>
        {
            RegisterEPASOptions(options);
        });
    }
    private static void RegisterEPASOptions(PcapPostgresOptions options)
    {
        options.MessageCatalog.AddOrReplaceBackendMessage(new('u', "OutDescription"));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('v', "ParamData"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('O', "ParseOut"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('u', "DescribeOut"));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('v', "ExecuteOut"));

        options.CustomMessageProcessor = HandleEdbEpasMessages;

        options.CustomTemplateProvider = HandleEdbEpasTemplates;
        options.CustomHeaderProvider = GetCustomHeader();
    }

    private static OutDescriptionMessage? LastOutDescriptionMessage;
    private static IPostgresMessage? HandleEdbEpasMessages(PostgresMessage pgMessage, ParserInfo info)
    {
        IPostgresMessage? message = pgMessage.Name! switch
        {
            nameof(ExecuteOut) => ExecuteOutMessage.Read(pgMessage.Code, info.Reader),
            nameof(DescribeOut) => DescribeOutMessage.Read(pgMessage.Code, info.Reader),
            nameof(ParseOut) => ParseOutMessage.Read(pgMessage.Code, info.Reader),
            nameof(OutDescription) => OutDescriptionMessage.Read(pgMessage.Code, info.Reader),
            "ParamData" => SendOutTupleMessage.Read(pgMessage.Code, info.Reader, LastOutDescriptionMessage),
            _ => null
        };

        if (message is OutDescriptionMessage outDescriptionMessage)
            LastOutDescriptionMessage = outDescriptionMessage;

        return message;
    }

    private static ITextTransformer? HandleEdbEpasTemplates(IPostgresMessage message) => message switch
    {
        ParseOutMessage m => new ParseOut(m),
        DescribeOutMessage m => new DescribeOut(m),
        ExecuteOutMessage m => new ExecuteOut(m),
        OutDescriptionMessage m => new OutDescription(m),
        SendOutTupleMessage m => new SendOutTuple(m),
        _ => null
    };

    private static Func<string?, GenerationState, ITextTransformer?> GetCustomHeader()
        => (string? message, GenerationState state) => new Header(message, state);
}
