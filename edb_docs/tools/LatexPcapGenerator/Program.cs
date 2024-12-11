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
        var result = Parser.Default.ParseArguments<CommandLineOptions>(args);
        result.WithParsed(opts =>
        {
            try
            {
                CheckInputFile(opts.InputFile);
                opts.OutputFile = CheckAndFixOutputFile(opts.InputFile, opts.OutputFile, opts.Multiple);

                // We have the parsed arguments, so let's just pass them down
                ProcessFile(opts.InputFile, opts.OutputFile, opts.Standalone ?? true, opts.Port, opts.Multiple);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error! " + ex.Message);
                var helpText = HelpText.AutoBuild(result, x => x, x => x);

                Console.WriteLine(helpText);

            }
        });

#if DEBUG
        if (Debugger.IsAttached) Console.ReadLine();
#endif
    }

    private static void ProcessFile(string inputFile, string outputPath, bool standalone, int port, bool multipleFiles)
    {
        Console.WriteLine("Wireshark to LaTeX converter - Copyright EnterpriseDB");
        if (multipleFiles)
        {
            Console.WriteLine($"Processing file '{Path.GetFileName(inputFile)}' as multiple standalone LaTeX documents...");
        }
        else
        {
            Console.WriteLine($"Processing file '{Path.GetFileName(inputFile)}' as {(standalone ? "standalone" : "article")} LaTeX document...");
        }
        Console.WriteLine($"Output path: {outputPath}");

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
            state = multipleFiles ? PcapToLatexService.PcapToLaTeX_MultipleFiles(packets, outputPath)
                : PcapToLatexService.PcapToLaTeX(packets, outputPath, standalone);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error has occurred: {ex.Message}");
            Console.WriteLine($"Messages may still have been processed and written to output file.");
        }
        finally
        {
            Console.WriteLine($"LaTeX file written to {outputPath}");
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

    private static string CheckAndFixOutputFile(string inputFile, string? outputFile, bool multiple)
    {
        inputFile = Path.GetFullPath(inputFile);
        if (outputFile == null)
            return multiple ? Path.Combine(Path.GetDirectoryName(inputFile)!, Path.GetFileNameWithoutExtension(inputFile)) : Path.ChangeExtension(inputFile, ".tex")!;

        outputFile = Path.GetFullPath(outputFile);
        bool isDirectory = string.IsNullOrEmpty(Path.GetExtension(outputFile));

        if (isDirectory)
            return multiple ? Path.Combine(outputFile, Path.GetFileNameWithoutExtension(Path.GetFileName(inputFile))) : Path.Combine(outputFile, Path.ChangeExtension(Path.GetFileName(inputFile), ".tex")!);

        return outputFile;
    }



}
