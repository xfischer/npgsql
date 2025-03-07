using Microsoft.Extensions.Logging;

namespace pgcap2latex.Infrastructure;

public static class SpectreConsoleLoggingExtensions
{
    public static ILoggingBuilder AddSpectreConsole(this ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.AddProvider(new SpectreConsoleLoggerProvider());
        return loggingBuilder;
    }

    //public static ILoggingBuilder AddSpectreConsole(this ILoggingBuilder loggingBuilder, Action<SpectreConsoleLoggerConfiguration> configure)
    //{
    //    var config = new SpectreConsoleLoggerConfiguration();
    //    configure(config);
    //    return loggingBuilder.AddSpectreConsole(config);
    //}        
}
