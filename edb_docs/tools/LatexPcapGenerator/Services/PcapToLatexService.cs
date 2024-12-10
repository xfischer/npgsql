using pcap2latex.Model;
using pcap2latex.Templates;
using pcap2latex.Templates.Paging;
using System.Runtime.Serialization;
using System.Text;

namespace pcap2latex;

public static class PcapToLatexService
{
    private const int MaxLatexRowsPerPage = 21;

    public static GenerationState PcapToLaTeX(IEnumerable<PostgresPacket> pgSqlPackets, string latexOutputFile, bool standalone = true)
    {
        GenerationState state = new(standalone: standalone);

        var fileLatexBuilder = new StringBuilder();
        int packetIndex = 1;

        try
        {
            foreach (var packet in pgSqlPackets)
            {
                state.LatexRowCount = 0;
                // Packet Footer
                if (packetIndex > 1)
                {
                    bool newChapter = state.LastMessage is ReadyForQueryMessage;

                    fileLatexBuilder.AppendLine(new PacketFooter(newChapter, state).TransformText());
                    state.LatexRowCount += (newChapter ? 1 : 0);
                }

                // Packet Header
                fileLatexBuilder.AppendLine(new PacketHeader(packet.Messages, packet.IsFrontEnd, packetIndex, state).TransformText());

                foreach (var pgMessage in packet.Messages)
                {
                    if (ProcessPostGresMessage(pgMessage, state, fileLatexBuilder))
                    {
                        state.StatsMesssagesProcessed++;
                    }
                    else
                    {
                        state.StatsMesssagesInvalid++;
                        fileLatexBuilder.AppendLine($"No message definition found for code '{pgMessage.GetType().Name}'");
                        fileLatexBuilder.AppendLine("\\\\");
                        Console.WriteLine($"No message definition found for code '{pgMessage.GetType().Name}'");
                    }
                }
                packetIndex++;
                state.StatsPacketsProcessed++;
            }
        }
        finally // write even if error occured
        {
            // Last packet Footer
            fileLatexBuilder.AppendLine(new PacketFooter(newChapter: false, state).TransformText());

            // Footer
            fileLatexBuilder.AppendLine(new Footer(state).TransformText());

            // Header INSERTION AT BEGINNING
            fileLatexBuilder.Insert(0, new Header($"PostgreSQL packets. {packetIndex - 1} packet(s).", state).TransformText() + Environment.NewLine);


            var finalLatex = fileLatexBuilder.ToString();
            File.WriteAllText(latexOutputFile, finalLatex);
        }

        return state;
    }

    public static GenerationState PcapToLaTeX_MultipleFiles(IEnumerable<PostgresPacket> pgSqlPackets, string latexOutputDirectory)
    {
        GenerationState state = new(standalone: true, multiple: true);

        int packetIndex = 1;
        if (!Directory.Exists(latexOutputDirectory))
            Directory.CreateDirectory(latexOutputDirectory);

        foreach (var packet in pgSqlPackets)
        {
            foreach (var pgMessage in packet.Messages)
            {
                var fileLatexBuilder = new StringBuilder();
                // Packet Header
                fileLatexBuilder.AppendLine(new PacketHeader(packet.Messages, packet.IsFrontEnd, packetIndex, state).TransformText());

                bool success = ProcessPostGresMessage(pgMessage, state, fileLatexBuilder, (builder, stateObj) =>
                {
                    state.StatsMesssagesProcessed++;
                    
                    // Last packet Footer
                    fileLatexBuilder.AppendLine(new PacketFooter(newChapter: false, state).TransformText());

                    // Footer
                    fileLatexBuilder.AppendLine(new Footer(state).TransformText());

                    // Header INSERTION AT BEGINNING
                    fileLatexBuilder.Insert(0, new Header(null, state).TransformText() + Environment.NewLine);

                    var finalLatex = fileLatexBuilder.ToString();
                    var fileName = Path.Combine(latexOutputDirectory, $"packet{packetIndex:0000}_message{state.StatsMesssagesProcessed:0000}.tex");
                    File.WriteAllText(fileName, finalLatex);
                    fileLatexBuilder.Clear();

                    // Packet Header
                    fileLatexBuilder.AppendLine(new PacketHeader(packet.Messages, packet.IsFrontEnd, packetIndex, state).TransformText());
                });

                if (!success)
                {
                    state.StatsMesssagesInvalid++;
                    fileLatexBuilder.AppendLine($"No message definition found for code '{pgMessage.GetType().Name}'");
                    fileLatexBuilder.AppendLine("\\\\");
                    Console.WriteLine($"No message definition found for code '{pgMessage.GetType().Name}'");
                }
            }
            packetIndex++;
            state.StatsPacketsProcessed++;
        }


        return state;
    }

    private static bool ProcessPostGresMessage(PostgresMessageBase message, GenerationState state, StringBuilder latexBuilder, Action<StringBuilder, GenerationState>? messageReadyAction = null)
    {
        // check consecutive datarows
        // if max datarows reached, skip until the last and write a "n skipped rows" skippedwords
        if (state.LastMessage is DataRowMessage)
        {
            if (message is DataRowMessage)
            {
                state.ConsecutiveDataRows++;
                if (state.ConsecutiveDataRows >= GenerationOptions.MaxDataRows)
                    return true;
            }
            else
            {
                if (state.ConsecutiveDataRows >= GenerationOptions.MaxDataRows)
                {
                    // next message after n datarows
                    ITextTransformer skippedWordsTransformer = new SkippedWords("DataRow", skippedItems: state.ConsecutiveDataRows);

                    WriteTextTransformation(state, latexBuilder, skippedWordsTransformer);

                    // Send event (for multiple mode)
                    messageReadyAction?.Invoke(latexBuilder, state);
                }
                state.ConsecutiveDataRows = 0;
                // continue
            }
        }

        state.LastMessage = message;

        ITextTransformer? textTransformer = FindTextTransformer(message);

        if (textTransformer == null)
        {
            latexBuilder.AppendLine($"No template found for '{message.GetType().Name}' \\\\");
            latexBuilder.AppendLine(new MessageSeparator().TransformText());
            return false;
        }

        WriteTextTransformation(state, latexBuilder, textTransformer);

        // Send event (for multiple mode)
        messageReadyAction?.Invoke(latexBuilder, state);

        return true;
    }

    private static void WriteTextTransformation(GenerationState state, StringBuilder latexBuilder, ITextTransformer textTransformer)
    {
        var estimatedRowCount = textTransformer.EstimateBytefieldRowCount();
        latexBuilder.AppendDebugLine($"% row count: {state.LatexRowCount}, estimated next: {estimatedRowCount}, new row count: {state.LatexRowCount + estimatedRowCount} (max: {MaxLatexRowsPerPage})");
        state.LatexRowCount += estimatedRowCount;

        if (state.LatexRowCount > MaxLatexRowsPerPage && !state.Standalone)
        {
            latexBuilder.AppendDebugLine($"% page break. row count: {state.LatexRowCount}, max: {MaxLatexRowsPerPage}");
            latexBuilder.AppendLine(new PacketFooter(newChapter: true, state, "Conversation (continuation)").TransformText());
            state.LatexRowCount = 1; // new chapter takes 1 row vertical space
            latexBuilder.AppendLine(new PacketHeader(state).TransformText());
        }

        latexBuilder.AppendLine(textTransformer.TransformText());
        latexBuilder.AppendLine(new MessageSeparator().TransformText());
    }

    private static ITextTransformer? FindTextTransformer(PostgresMessageBase message) => message switch
    {
        QueryMessage m => new Query(m),
        ParseMessage m => new Parse(m),
        ParseOutMessage m => new ParseOut(m),
        DescribeMessage m => new Describe(m),
        SyncMessage _ => new Sync(),
        NoDataMessage _ => new NoData(),
        BindCompleteMessage _ => new BindComplete(),
        ParseCompleteMessage m => new ParseComplete(m),
        ParameterDescriptionMessage m => new ParameterDescription(m),
        RowDescriptionMessage m => new RowDescription(m),
        ReadyForQueryMessage m => new ReadyForQuery(m),
        BindMessage m => new Bind(m),
        DescribeOutMessage m => new DescribeOut(m),
        ExecuteMessage m => new Execute(m),
        ExecuteOutMessage m => new ExecuteOut(m),
        OutDescriptionMessage m => new OutDescription(m),
        DataRowMessage m => new DataRow(m),
        CommandCompleteMessage m => new CommandComplete(m),
        NoticeResponseMessage m => new NoticeResponse(m),
        SendOutTupleMessage m => new SendOutTuple(m),
        TerminateMessage _ => new Terminate(),
        SSLRequestMessage m => new SSLRequest(m),
        SSLResponseMessage m => new SSLResponse(m),
        StartupMessageMessage m => new StartupMessage(m),
        AuthenticationGenericMessage m => new AuthenticationGeneric(m),
        SASLInitialResponseMessage m => new SASLInitialResponse(m),
        SASLResponseMessage m => new SASLResponse(m),
        ParameterStatusMessage m => new ParameterStatus(m),
        BackendKeyDataMessage m => new BackendKeyData(m),
        ErrorResponseMessage m => new ErrorResponse(m),
        _ => null,
    };
}
