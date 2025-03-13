using System.Text;
using Microsoft.Extensions.Logging;
using pcap2latex;

string inputFile = "extendedQuery.pcapng";
ushort portNumber = 5432;

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger logger = factory.CreateLogger<Program>();
logger.LogInformation("Starting...");

// Capture
var pcapOptions = new PcapPostgresOptions().AddDefaultPostgresMessages();
var pcap = PcapService.Create(factory, pcapOptions);
var pgPackets = pcap.ConvertPcap(inputFile, portNumber).ToList();

logger.LogInformation("{Count} packet(s) retrieved", pgPackets.Count);


// Transform to LaTeX
var latexFile = Path.ChangeExtension(inputFile, ".tex");
var latexOptions = new PcapToLatexOptions();
var latex = PcapToLatexService.Create(factory, latexOptions);
latex.PcapToLaTeX(pgPackets, latexFile, standalone: true);
logger.LogInformation("LaTeX file written to {File}", Path.GetFullPath(latexFile));


// Transform to anything!
// here PlantUML sequence diagram in Markdown
var markdownFile = Path.ChangeExtension(inputFile, ".md");
var markdown = PacketsToMarkdown(pgPackets);

await File.WriteAllTextAsync(markdownFile, markdown);
logger.LogInformation("Markdown/PlantUML file written to {File}", Path.GetFullPath(markdownFile));

string PacketsToMarkdown(List<PostgresPacket> pgPackets)
{
    var markdown = new StringBuilder();
    markdown.AppendLine($"""
            # Sequence diagram

            Recorded : {pgPackets[0].Timestamp:O}

            ## Compact diagram

            """);

    markdown.AppendLine(PacketsToMarkdownUml(pgPackets, compact: true));

    markdown.AppendLine("""
        ## Detailed diagram

        """);
    markdown.AppendLine(PacketsToMarkdownUml(pgPackets, compact: false));

    return markdown.ToString();
}

static string PacketsToMarkdownUml(List<PostgresPacket> pgPackets, bool compact)
{
    var markdown = new StringBuilder();
    markdown.AppendLine($"""
            ```plantuml
            @startuml
            participant "Client\n{pgPackets[0].SourceAddress}:{pgPackets[0].SourcePort}" as C
            database "Server\n{pgPackets[0].DestinationAddress}:{pgPackets[0].DestinationPort}" as S
            """);

    int packetIndex = 0;
    DateTime initialDate = default;
    DateTime lastDate = default;
    foreach (var packet in pgPackets)
    {
        if (!compact)
        {
            if (packetIndex == 0)
            {
                initialDate = packet.Timestamp;
                markdown.AppendLine($"group packet {++packetIndex}");
            }
            else
            {
                // delta time
                markdown.Append($"group packet {++packetIndex} [");
                markdown.Append($"{(packet.Timestamp - initialDate).TotalMilliseconds:N1} ms TOTAL");
                markdown.AppendLine($"\\n+{(packet.Timestamp - lastDate).TotalMilliseconds:N1} ms SINCE last]");
            }
            lastDate = packet.Timestamp;
        }

        markdown.AppendLine(PacketToUml(packet, compact));

        if (!compact)
        {
            markdown.AppendLine("end");
        }
    }

    markdown.AppendLine("""
            @enduml
            ```
            """);

    return markdown.ToString();
}

static string? PacketToUml(PostgresPacket packet, bool compact)
{
    var strOutBuilder = new StringBuilder();
    List<(PostgresMessageBase Message, int Count)> messagesGrouped = new();
    foreach (var message in packet.Messages)
    {
        if (messagesGrouped.Count > 0 && messagesGrouped[^1].Message.Code == message.Code)
        {
            messagesGrouped[^1] = (messagesGrouped[^1].Message, messagesGrouped[^1].Count + 1);
        }
        else
        {
            messagesGrouped.Add((message, 1));
        }
    }

    if (compact)
    {
        strOutBuilder.Append(packet.IsFrontEnd ? "C -> S : " : "S -> C : ");
        strOutBuilder.AppendLine(string.Join(" / ", messagesGrouped.Select(m => 
                                    m.Count == 1 ? 
                                    m.Message.Name 
                                    : $"{m.Message.Name} (x{m.Count})"
                                    )));
    }
    else
    {
        foreach (var msg in messagesGrouped)
        {
            strOutBuilder.Append(packet.IsFrontEnd ? "C -> S : " : "S -> C : ");
            strOutBuilder.AppendLine(msg.Count == 1 ? msg.Message.Name : $"{msg.Message.Name} (x{msg.Count})");
        }
    }
    return strOutBuilder.ToString();
}



