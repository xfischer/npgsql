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
        return services.AddPcap2Latex(ConfigureEPASCaptureOptions, ConfigureEPASLatexOptions);
    }
    private static void ConfigureEPASCaptureOptions(PcapPostgresOptions options)
    {
        options.MessageCatalog.AddOrReplaceBackendMessage(new('u', "OutDescription", IsFrontEnd: false));
        options.MessageCatalog.AddOrReplaceBackendMessage(new('v', "ParamData", IsFrontEnd: false));

        options.MessageCatalog.AddOrReplaceFrontendMessage(new('O', "ParseOut", IsFrontEnd: true));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('u', "DescribeOut", IsFrontEnd: true));
        options.MessageCatalog.AddOrReplaceFrontendMessage(new('v', "ExecuteOut", IsFrontEnd: true));

        options.CustomMessageProcessor = HandleEdbEpasMessages;
    }
    private static void ConfigureEPASLatexOptions(PostgresToLatexOptions options)
    {
        options.CustomTemplateProvider = HandleEdbEpasTemplates;
        options.CustomHeaderProvider = GetCustomHeader();
    }

    private static OutDescriptionMessage? LastOutDescriptionMessage;
    private static PostgresMessageBase? HandleEdbEpasMessages(PostgresMessage pgMessage, ParserInfo info)
    {
        PostgresMessageBase? message = pgMessage.Name! switch
        {
            nameof(ExecuteOut) => ExecuteOutMessage.Read(pgMessage, info.Reader),
            nameof(DescribeOut) => DescribeOutMessage.Read(pgMessage, info.Reader),
            nameof(ParseOut) => ParseOutMessage.Read(pgMessage, info.Reader),
            nameof(OutDescription) => OutDescriptionMessage.Read(pgMessage, info.Reader),
            "ParamData" => SendOutTupleMessage.Read(pgMessage, info.Reader, LastOutDescriptionMessage),
            _ => null
        };

        if (message is OutDescriptionMessage outDescriptionMessage)
            LastOutDescriptionMessage = outDescriptionMessage;

        return message;
    }

    private static ITextTransformer? HandleEdbEpasTemplates(PostgresMessageBase message) => message switch
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
