using Microsoft.Extensions.Logging;
using pcap2latex;

namespace pgcap2latex;

public sealed class ConvertApp(PcapService pcapService, 
                                PcapToLatexService pcapToLatexService,
                                ILogger<ConvertApp> logger)
{
    public void ProcessFile(string inputFile, string outputPath, bool standalone, ushort port, bool multipleFiles)
    {
        logger.LogInformation("PCAP to LaTeX converter - Copyright EnterpriseDB");

        if (multipleFiles)
        {
            logger.LogInformation("Processing file '{File}' as multiple standalone LaTeX documents...", Path.GetFileName(inputFile));
        }
        else
        {
            logger.LogInformation("Processing file '{File}' as {Standalone} LaTeX document...", Path.GetFileName(inputFile), (standalone ? "standalone" : "article"));
        }
        logger.LogInformation("Output path {OutputPath}", outputPath);

        GenerationState? state = null;
        try
        {
            IEnumerable<PostgresPacket> packets = pcapService.ConvertPcap(inputFile, port);

            state = multipleFiles ? pcapToLatexService.PcapToLaTeX_MultipleFiles(packets, outputPath)
                : pcapToLatexService.PcapToLaTeX(packets, outputPath, standalone);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error has occurred: {Message}", ex.Message);
            logger.LogError("Messages may still have been processed and written to output file.");
        }
        finally
        {
            logger.LogInformation("LaTeX file written to {OutputPath}", outputPath);
            if (state != null)
            {
                logger.LogInformation("{StatsPacketsProcessed} packet(s) processed. {StatsMesssagesProcessed} messages written.", state.StatsPacketsProcessed, state.StatsMesssagesProcessed);
                if (state.StatsMesssagesInvalid > 0)
                {
                    logger.LogWarning("{StatsMesssagesInvalid} ignored or invalid.", state.StatsMesssagesInvalid);
                }
            }
        }
    }

    internal void ProcessFileAsText(string inputFile, string outputPath, ushort port)
    {
        logger.LogInformation("PCAP to Text converter - Copyright EnterpriseDB");

        logger.LogInformation("Processing file '{File}' as text...", Path.GetFileName(inputFile));
        logger.LogInformation("Output path {OutputPath}", outputPath);

        GenerationState? state = null;
        try
        {
            IEnumerable<PostgresPacket> packets = pcapService.ConvertPcap(inputFile, port);

            using var writer = new StreamWriter(outputPath, false);
            foreach (var p in packets)
            {
                writer.WriteLine($"Packet ({(p.IsFrontEnd ? 'F' : 'B')})");
                foreach (var m in p.Messages)
                {
                    writer.Write(p.Timestamp.ToString("O"));
                    writer.Write('\t');
                    writer.Write(m.FrontEnd ? 'F' : 'B');
                    writer.Write('\t');
                    writer.Write(m.Length);
                    writer.Write('\t');
                    writer.Write(m.Name);
                    writer.Write('\t');
                    writer.WriteLine(m.GetStringRepresentation());
                }
            }            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error has occurred: {Message}", ex.Message);
            logger.LogError("Messages may still have been processed and written to output file.");
        }
        finally
        {
            logger.LogInformation("File written to {OutputPath}", outputPath);
            if (state != null)
            {
                logger.LogInformation("{StatsPacketsProcessed} packet(s) processed. {StatsMesssagesProcessed} messages written.", state.StatsPacketsProcessed, state.StatsMesssagesProcessed);
                if (state.StatsMesssagesInvalid > 0)
                {
                    logger.LogWarning("{StatsMesssagesInvalid} ignored or invalid.", state.StatsMesssagesInvalid);
                }
            }
        }
    }
}
