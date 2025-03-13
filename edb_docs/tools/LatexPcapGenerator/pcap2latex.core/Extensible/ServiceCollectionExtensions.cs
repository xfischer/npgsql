using Microsoft.Extensions.DependencyInjection;

namespace pcap2latex;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPcap2Latex(
        this IServiceCollection services
        , Action<PcapPostgresOptions>? captureOptions = null
        , Action<PcapToLatexOptions>? transformOptions = null)
    {

        services.Configure<PcapPostgresOptions>(opts =>
        {
            opts.AddDefaultPostgresMessages();
        });

        if (captureOptions is not null)
        {
            services.PostConfigure(captureOptions);
        }
        if (transformOptions is not null)
        {
            services.PostConfigure(transformOptions);
        }

        services.AddTransient<IPcapService, PcapService>()
                .AddTransient<IPcapToLatexService, PcapToLatexService>();

        return services;
    }
}

