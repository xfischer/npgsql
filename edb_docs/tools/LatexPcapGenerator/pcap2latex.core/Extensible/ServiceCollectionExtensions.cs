using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using pcap2latex;

namespace pcap2latex;

public static class Pcap2LatexServiceCollectionExtensions
{
    public static IServiceCollection AddPcap2Latex(
        this IServiceCollection services
        , Action<PcapPostgresOptions>? options = null)
    {

        services.Configure<PcapPostgresOptions>(opts =>
        {
            opts.AddDefaultPostgresMessages();
        });

        if (options is not null)
        {
            services.PostConfigure(options);
        }

        services.AddTransient<PcapService>()
                .AddTransient<PcapToLatexService>();

        return services;
    }

    
}

