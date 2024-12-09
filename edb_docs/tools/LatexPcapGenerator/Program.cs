using CommandLine;
using CommandLine.Text;
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
        HelpText oHelpText;
        ParserResult<CommandLineOptions> oResult;

        oResult = Parser.Default.ParseArguments<CommandLineOptions>(args);
        oResult.WithParsed(opts =>
        {
            try
            {
                CheckInputFile(opts.InputFile);
                opts.OutputFile = CheckOutputFile(opts.InputFile, opts.OutputFile);

                // We have the parsed arguments, so let's just pass them down
                ProcessFile(opts.InputFile, opts.OutputFile, opts.Standalone ?? true, opts.Port);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error! " + ex.Message);
                oHelpText = HelpText.AutoBuild(oResult, x => x, x => x);

                Console.WriteLine(oHelpText);
#if DEBUG
                if (Debugger.IsAttached) Console.ReadLine();
#endif
            }
        });

        //oHelpText = HelpText.AutoBuild(oResult, x => x, x => x);

        //Console.WriteLine(oHelpText);

    }

    private static void ProcessFile(string inputFile, string outputFile, bool standalone, int port)
    {
        Console.WriteLine("Wireshark to LaTeX converter - Copyright EnterpriseDB");
        Console.WriteLine($"Processing file '{Path.GetFileName(inputFile)}' as {(standalone ? "standalone" : "article")} LaTeX document...");

        GenerationState? state = null;
        try
        {
            IEnumerable<PostgresPacket> packets;
            if (Path.GetExtension(inputFile).ToLower() == ".pdml")
            {
                packets = PdmlService.ConvertPdmlToPcap(inputFile);
            }
            else
            {
                packets = PcapService.ConvertPcap(inputFile, (ushort)port);
            }

#if DEBUG
            var packetList = new List<PostgresPacket>();
            foreach (PostgresPacket packet in packets)
            {
                packetList.Add(packet);
            }
            packets = packetList;
#endif
            state = PcapToLatexService.PcapToLaTeX(packets, outputFile, standalone);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error has occurred: {ex.Message}");
            Console.WriteLine($"Messages may still have been processed and written to output file.");
        }
        finally
        {
            Console.WriteLine($"LaTeX file written to {outputFile}");
            if (state != null)
            {
                Console.Write($"{state.StatsPacketsProcessed} packet(s) processed. {state.StatsMesssagesProcessed} messages written");
                Console.WriteLine(state.StatsMesssagesInvalid > 0
                    ? $", {state.StatsMesssagesInvalid} ignored or invalid."
                    : ".");
            }
        }
    }

    private static bool CheckInputFile(string inputFile)
    {
        if (!File.Exists(inputFile))
        {
            throw new ArgumentException($"Input file {inputFile} does not exists.");
        }
        List<string> supportedFileTypes = [".pdml", ".pcap", ".pcapng"];
        var fileExt = Path.GetExtension(inputFile).ToLower();
        if (!supportedFileTypes.Contains(fileExt))
        {
            throw new ArgumentException($"Non supported input file. Supported types are {string.Join(", ", supportedFileTypes)}.");
        }
        return true;
    }

    private static string CheckOutputFile(string inputFile, string? outputFile)
    {
        if (outputFile == null)
            return Path.ChangeExtension(inputFile, ".tex");
        return outputFile;
    }



}
