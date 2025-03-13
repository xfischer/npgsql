using Microsoft.Extensions.Logging;
using pcap2latex;

namespace epascap2latex;

public sealed class ConvertApp(IPcapService pcapService, 
                                IPcapToLatexService pcapToLatexService,
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
}
