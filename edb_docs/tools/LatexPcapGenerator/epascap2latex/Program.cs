using Microsoft.Extensions.DependencyInjection;
using pcap2latex;
using epascap2latex.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace epascap2latex;

internal sealed class Program
{
    static int Main(string[] args)
    {
        try
        {
            // to retrieve the log file name, we must first parse the command settings
            // this will require us to delay setting the file path for the file writer.
            // With serilog we can use an enricher and Serilog.Sinks.Map to dynamically
            // pull this setting.
            var serviceCollection = new ServiceCollection()
                .AddLogging(configure =>
                    configure.AddSpectreConsole());

            serviceCollection.AddPcap2Latex_EPAS();
            serviceCollection.AddSingleton<ConvertApp>();

            var registrar = new TypeRegistrar(serviceCollection);
            var app = new CommandApp<ConvertCommand>(registrar);
            app.Configure(config =>
            {
                config.SetApplicationName("epascap2latex");
#if DEBUG
                config.PropagateExceptions();
                config.ValidateExamples();
#endif
                config.AddExample("file.pcapng", "diagram.tex");
                config.AddExample("file.pcapng", "diagram.tex", "5444", "--standalone");
                config.AddExample("file.pcapng", "diagram.tex", "5444", "--standalone", "--multiple");


            });
            return app.Run(args);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return -1;
        }
    }
}
