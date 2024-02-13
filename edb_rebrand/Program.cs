

using Microsoft.VisualBasic;

namespace edb_rebrand
{
    internal class Program
    {
        static readonly string[] fileExtensionsIncludeOnlyFilter = [".cs", ".snbtxt", ".txt"];
        static readonly string[] nameExcludeFilter = ["AssemblyInfo.cs"];
        static readonly string[] directoryExcludeFilter = ["bin", "obj", ".vs", ".git"];
        static void Main(string[] args)
        {
            if (args.Length == 0
                || args[0].Equals("-h", StringComparison.OrdinalIgnoreCase)
                || args[0].Equals("-help", StringComparison.OrdinalIgnoreCase))
            {
                ShowHelp();
                return;
            }

            var fileOrDir = args[0];
            Report? report = null;
            if (Directory.Exists(fileOrDir))
            {
                report = ProcessDirectory(fileOrDir);
            }
            else
            {
                report = ProcessFile(fileOrDir);
            }

            PrintReport(report);

            Console.WriteLine($"Done ! {report?.numFilesProcessed} file(s), {report?.numFilesIgnored} file(s) ignored, {report?.numDirectoryProcessed} directory(ies), {report?.NewContentsByFile.Sum(_ => _.Value.NumOccurences)} occurence(s) in total.");

            if (report?.numFilesProcessed > 0)
            {
                Console.Write($"Apply changes (y/n): ");
                var key = Console.ReadKey(true);

                if (key.KeyChar == 'y')
                {
                    ApplyReportChanges(report);
                }
            }
            else
            {
                Console.WriteLine($"No changes to apply.");
            }

        }

        private static void ApplyReportChanges(Report? report)
        {
            foreach (var item in report?.NewContentsByFile!)
            {
                Console.WriteLine("Saving file " + item.Key);
                File.WriteAllText(item.Key, item.Value.NewContent);
            }
        }

        private static void PrintReport(Report? report)
        {
            foreach (var item in report?.NewContentsByFile!)
            {
                Console.WriteLine($"{item.Key} : {item.Value.NumOccurences} occurence(s) found");
            }
        }

        private static Report ProcessFile(string file)
        {
            if (!File.Exists(file)
                || !fileExtensionsIncludeOnlyFilter.Contains(Path.GetExtension(file))
                || nameExcludeFilter.Contains(Path.GetFileName(file))
                )
            {
                return new Report() { numDirectoryProcessed = 0, numFilesProcessed = 0, numFilesIgnored = 1 };
            }

            var contents = File.ReadAllText(file);
            var newContents = contents.Replace("Npgsql;", "EnterpriseDB.EDBClient;", StringComparison.Ordinal)
                                    .Replace("Npgsql.", "EnterpriseDB.EDBClient.", StringComparison.Ordinal)
                                    .Replace("Npgsql ", "EDB .NET Connector ", StringComparison.Ordinal)
                                    .Replace("Npgsql", "EDB", StringComparison.Ordinal);

            if (newContents != contents)
            {
                var numOccurences = contents.Count("Npgsql;")
                                    + contents.Count("Npgsql.")
                                    + contents.Count("Npgsql ");
                var distinctNumOccurences = contents.Count("Npgsql") - numOccurences;
                if (distinctNumOccurences == 0)
                {
                    distinctNumOccurences = numOccurences;
                }

                return new Report() { numDirectoryProcessed = 0, numFilesProcessed = 1, numFilesIgnored = 0, NewContentsByFile = new() { { file, (newContents, distinctNumOccurences) } } };
            }
            return new Report() { numDirectoryProcessed = 0, numFilesProcessed = 0, numFilesIgnored = 1 };

        }

        private static Report ProcessDirectory(string fileOrDir)
        {
            var dirName = new DirectoryInfo(fileOrDir).Name;
            if (directoryExcludeFilter.Contains(dirName.ToLower()))
            {
                return new Report() { numDirectoryProcessed = 0, numFilesProcessed = 0, numFilesIgnored = 1 };
            }

            Report report = new();
            foreach (var file in Directory.GetFiles(fileOrDir))
            {
                report += ProcessFile(file);
            }
            foreach (var directory in Directory.GetDirectories(fileOrDir))
            {
                report += ProcessDirectory(directory);
            }
            report.numDirectoryProcessed++;

            return report;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("""
                edb_rebrand: EDB rebranding tool for NpgSql
                Takes a file or directory as argument and will rebrand Npgsql to EDB
                """);
        }
    }
}
