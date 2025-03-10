using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pcap2latex;

namespace pcap2latex;

public static class Bootstrapper
{
    public static IServiceCollection AddPcap2Latex(
        this IServiceCollection services
        , Action<PcapPostgresOptions>? captureOptions = null
        , Action<PostgresToLatexOptions>? transformOptions = null)
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

        services.AddTransient<PcapService>()
                .AddTransient<PcapToLatexService>();

        return services;
    }


    public static PcapService CreatePcapService(ILoggerFactory loggerFactory)
    {
        
        var options = new PcapPostgresOptions();
        options.AddDefaultPostgresMessages();      
        
        return new PcapService(loggerFactory.CreateLogger<PcapService>(), Options.Create(options));
    }

    public static PcapToLatexService CreatePgToLatexService(ILoggerFactory loggerFactory)
    {
        var options = new PostgresToLatexOptions();

        return new PcapToLatexService(loggerFactory.CreateLogger<PcapToLatexService>(), Options.Create(options));
    }
}

