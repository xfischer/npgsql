using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcap2latex;

public class CommandLineOptions
{
    [Value(index: 0, Required = true, HelpText = "Capture file to translate (.pcapng, .pdml)")]
    public string InputFile { get; set; }

    [Option(shortName: 'o', longName: "output", Required = false, Default =null, HelpText ="Output file path. Leave empty generate a file at the same location as input file, with .tex extension")]
    public string? OutputFile { get; set; }

    [Option(shortName: 's', longName: "standalone", Required = true, HelpText = "True for standlone LaTeX, ideal for short messages. False will generate LaTeX article breaking pages when possible", Default = true)]
    public bool? Standalone { get; set; }

    [Option(shortName: 'p', longName: "port", Required = true, Default = 5432, HelpText = "PostgreSQL port number. Only packets from/to this port will be processed.")]
    public int Port { get; set; }


}