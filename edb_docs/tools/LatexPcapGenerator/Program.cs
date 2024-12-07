using PacketDotNet;
using pcap2latex.Model;
using pcap2latex.Templates;
using pcap2latex.Templates.Paging;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace pcap2latex;

internal class Program
{
    static void Main(string[] args)
    {
        try
        {

            if (!CheckArgs(args, out bool standalone, out int portNumber))
            {
                ShowHelp();
                return;
            }

            Console.WriteLine("Wireshark to LaTeX converter - Copyright EnterpriseDB");
            Console.WriteLine($"Processing file '{Path.GetFileName(args[0])}' as {(standalone ? "standalone" : "article")} LaTeX document...");

            IEnumerable<PostgresPacket> packets;
            if (Path.GetExtension(args[0]).ToLower() == ".pdml")
            {
                packets = PdmlService.ConvertPdmlToPcap(args[0]);
            }
            else
            {
                packets = PcapService.ConvertPcap(args[0], (ushort)portNumber);
            }
//#if DEBUG
//            packets = packets.ToList();
//#endif

            var state = PcapToLatexService.PcapToLaTeX(packets, args[1], standalone);

            Console.WriteLine($"LaTeX file written to {args[1]}");
            Console.Write($"{state.StatsPacketsProcessed} packet(s) processed. {state.StatsMesssagesProcessed} messages written");
            Console.WriteLine(state.StatsMesssagesInvalid > 0
                ? $", {state.StatsMesssagesInvalid} ignored or invalid."
                : ".");                            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: " + ex.Message);
        }

#if DEBUG
        if (Debugger.IsAttached) Console.ReadLine();
#endif
    }

    private static void ShowHelp()
    {

        Console.WriteLine(
            """
            Wireshark to LaTeX converter - Copyright EnterpriseDB
            - will convert only PostgreSQL messages - 

            Usage: pcap2latex <input_file> <output_file> [-port:5432] [-s:1|0]

            input_file: Any PDML, PCAP, PCAPNG file containing PostgreSQL messages
            output_file: Destination LaTeX file (.tex)
            -port (optional, defaults to 5432): postgres port. All packets to or from this port will be processed
            -s (optional, defaults to standalone): generate as standalone document (1) or article (0). Article are better for long conversations
            """);
    }

    private static bool CheckArgs(string[] args, out bool standalone, out int port)
    {
        standalone = true;
        port = 5432;

        if (args.Length < 2)
        {
            Console.WriteLine("Two arguments expected!");
            return false;
        }

        if (!File.Exists(args[0]))
        {
            Console.WriteLine("Input file does not exist!");
            return false;
        }

        List<string> supportedFileTypes = [".pdml", ".pcap", ".pcapng"];
        var fileExt = Path.GetExtension(args[0]).ToLower();
        if (!supportedFileTypes.Contains(fileExt))
        {
            Console.WriteLine($"Non supported input file. Supported types are {string.Join(", ", supportedFileTypes)}.");
            return false;
        }

        if (Path.GetExtension(args[1]).ToLower() != ".tex")
        {
            Console.WriteLine("Output file must be a LaTeX .tex file!");
            return false;
        }

        if (args.Length >= 3)
        {
            FindArgs(args[2], ref standalone, ref port);
        }
        if (args.Length == 4)
        {
            FindArgs(args[3], ref standalone, ref port);
        }

        return true;

        static void FindArgs(string arg, ref bool standalone, ref int port)
        {
            if (arg.StartsWith("-s:"))
            {
                if (arg.Length != 4)
                    return;

                bool? standaloneArg = arg.ToLower() switch
                {
                    "-s:0" => false,
                    "-s:1" => true,
                    _ => null
                };
                if (standaloneArg == null)
                    return;

                standalone = standaloneArg.Value;
            }
            else if (arg.StartsWith("-port:") && arg.Length > 6)
            {
                if (int.TryParse(arg[6..], out int portNumber))
                {
                    port = portNumber;
                }
            }
        }
    }


}
