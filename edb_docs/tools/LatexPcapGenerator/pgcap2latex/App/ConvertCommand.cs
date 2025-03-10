using System;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace pgcap2latex;

internal sealed class ConvertCommand(ConvertApp app) : Command<ConvertCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Capture file to translate (.pcapng, .pcap)")]
        [CommandArgument(0, "<capture_file>")]
        public required string InputFile { get; init; }

        [Description("Output file path. Leave empty generate a file at the same location as input file, with .tex extension")]
        [CommandArgument(1, "[output_path]")]
        public string? OutputPath { get; private set; }

        [Description("PostgreSQL port number. Only packets from/to this port will be processed. Defaults to 5432")]
        [CommandArgument(2, "[postgres_port]")]
        [DefaultValue((ushort)5432)]
        public ushort? Port { get; set; }

        [Description("When set will generate standlone LaTeX documents, ideal for short messages. Leave unset to generate LaTeX articles with page breaks when possible")]
        [CommandOption("-s|--standalone")]
        [DefaultValue(false)]
        public bool? Standalone { get; init; } 

        [Description("When set, one file is generated per message in standalone mode")]
        [CommandOption("-m|--multiple")]
        [DefaultValue(false)]
        public bool? Multiple { get; init; }

        [Description("When set will generate text file (like PQTrace)")]
        [CommandOption("-t|--text")]
        [DefaultValue(false)]
        public bool? ToText { get; init; }

        public override ValidationResult Validate()
        {
            if (!CheckInputFile(InputFile, out var result))
                return result;

            OutputPath = CheckAndFixOutputFile(InputFile!, OutputPath, Multiple ?? false);

            if (Port is null
               || Port < 0
               || Port > ushort.MaxValue)
                return ValidationResult.Error("Port number is invalid!");

            return ValidationResult.Success();
        }

        static bool CheckInputFile(string inputFile, out ValidationResult result)
        {
            result = ValidationResult.Success();
            if (string.IsNullOrWhiteSpace(inputFile))
            {
                result = ValidationResult.Error($"Input file argument missing.");
                return false;
            }

            if (!File.Exists(inputFile))
            {
                result = ValidationResult.Error($"Input file {inputFile} does not exists.");
                return false;
            }

            List<string> supportedFileTypes = [".pcap", ".pcapng"];
            var fileExt = Path.GetExtension(inputFile).ToLower();
            if (!supportedFileTypes.Contains(fileExt))
            {
                result = ValidationResult.Error($"Non supported input file. Supported types are {string.Join(", ", supportedFileTypes)}.");
                return false;
            }

            return true;
        }

        static string CheckAndFixOutputFile(string inputFile, string? outputFile, bool multiple)
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

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        if (settings.ToText ?? false)
        {
            app.ProcessFileAsText(settings.InputFile!, settings.OutputPath!, settings.Port!.Value);
        }
        else
        {
            app.ProcessFile(settings.InputFile!, settings.OutputPath!, settings.Standalone ?? true, settings.Port!.Value, settings.Multiple ?? false);
        }

        return 0;
    }

    

}