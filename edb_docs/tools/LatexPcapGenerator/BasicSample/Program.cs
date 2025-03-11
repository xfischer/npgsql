using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using pcap2latex;

namespace BasicSample;

internal class Program
{
    static void Main(string[] args)
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger<Program>();
        logger.LogInformation("Starting...");

        // Capture
        var pcapOptions = new PcapPostgresOptions();
        pcapOptions.AddDefaultPostgresMessages();

        var pcap = PcapService.Create(factory, pcapOptions);
        var pgPackets = pcap.ConvertPcap("extendedQuery.pcapng", 5446).ToList();


        // Transform to LaTeX
        var latexOptions = new PcapToLatexOptions();
        var latex = PcapToLatexService.Create(factory, latexOptions);
        latex.PcapToLaTeX(pgPackets, "test.tex", standalone: false);

    }
}
