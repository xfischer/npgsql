using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;

namespace JoinBenchmarks;
internal class Program
{
    const string FilePattern = "*-report.csv";

    const char CsvSeparator = ',';
    readonly static string[] columnsToKeep = ["Method", "Runtime", "Mean [μs]", "Error [μs]", "StdDev [μs]", "Gen0", "Allocated [KB]", "Mean", "Error", "StdDev", "Allocated"];
    readonly static HashSet<string> columnsToRemove = ["AnalyzeLaunchVariance", "EvaluateOverhead", "MaxAbsoluteError", "MaxRelativeError", "MinInvokeCount", "MinIterationTime", "OutlierMode", "Affinity", "EnvironmentVariables", "Jit", "LargeAddressAware", "Platform" ,"PowerPlanMode","AllowVeryLargeObjects","Concurrent","CpuGroups","Force","HeapAffinitizeMask","HeapCount","NoAffinitize","RetainVm","Server","Arguments","BuildConfiguration","Clock","EngineFactory","NuGetReferences","IsMutator","InvocationCount","IterationCount","IterationTime","LaunchCount","MaxIterationCount","MaxWarmupIterationCount","MemoryRandomization","MinIterationCount","MinWarmupIterationCount","RunStrategy","UnrollFactor","WarmupCount"
        ];


    private static string GetConnectorFromPath(string file)
    {
        var dir = Directory.GetParent(Path.GetDirectoryName(file)!)!.Name;
        return dir switch
        {
            "BenchmarkDotNet.Artifacts.BenchmarkVersion7" => "7.0.6.2",
            "BenchmarkDotNet.Artifacts.BenchmarkVersion8" => "8.0.5.2",
            "BenchmarkDotNet.Artifacts.BenchmarkVersion9" => "9.0.2.1",
            "BenchmarkDotNet.Artifacts.BenchmarkVersion4" => "4.0.10.2",
            "BenchmarkDotNet.Artifacts.BenchmarkVersion10" => "10.0.1.1",
            "BenchmarkDotNet.Artifacts.BenchmarkNpgsql" => "Npgsql8",
            "BenchmarkDotNet.Artifacts.BenchmarkNetStandard" => "netstandard20",
            _ => "Unknown"
        };
    }

    static void Main(string[] args)
    {
        if (!CheckArgs(args))
            return;

        string path = args[0];

        //JoinOneFilePerHeader(path);

        JoinUnionAllHeaders(path);

        //JoinWithSameHeader(path);

#if DEBUG
        Console.ReadLine();
#endif
    }

    private static void JoinUnionAllHeaders(string path)
    {
        Dictionary<string, int> cumulativeHeaders = new Dictionary<string, int>();
        cumulativeHeaders.Add("Connector", 0);


        Console.WriteLine($"Processing headers");
        // Build headers
        foreach (var file in Directory.EnumerateFiles(path, FilePattern, SearchOption.AllDirectories))
        {
            using var inputFile = new StreamReader(file, encoding: Encoding.UTF8, false, new FileStreamOptions() { Access = FileAccess.Read, Mode = FileMode.Open, Share = FileShare.Read });
            var currentHeader = inputFile.ReadLine()!.Split(CsvSeparator);
            inputFile.Close();

            foreach (var colName in currentHeader)
            {
                if (!cumulativeHeaders.ContainsKey(colName) && !columnsToRemove.Contains(colName))
                {
                    cumulativeHeaders.Add(colName, cumulativeHeaders.Count);
                }
            }
        }

        // Create output file
        string mergeFilePath = Path.Combine(path, $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}-mergedreport.csv");
        using (var mergeFileStream = new StreamWriter(mergeFilePath, append: false, Encoding.UTF8))
        {

            // write header
            mergeFileStream.WriteLine(string.Join(CsvSeparator, cumulativeHeaders.Keys));

            Dictionary<int, int?> newColIndex = new Dictionary<int, int?>();

            foreach (var file in Directory.EnumerateFiles(path, FilePattern, SearchOption.AllDirectories))
            {
                Console.WriteLine($"Processing file {Path.GetFileName(file)}");
                using var inputFile = new StreamReader(file, encoding: Encoding.UTF8, false, new FileStreamOptions() { Access = FileAccess.Read, Mode = FileMode.Open, Share = FileShare.Read });
                var currentHeader = inputFile.ReadLine()!.Split(CsvSeparator);

                for (int i = 0; i < currentHeader.Length; i++)
                {
                    if (cumulativeHeaders.TryGetValue(currentHeader[i], out var newIndex))
                    {
                        newColIndex[i] = newIndex;
                    }
                    else
                    {
                        newColIndex[i] = null;
                    }
                }


                string[] newRow = new string[cumulativeHeaders.Count];
                do
                {
                    var row = ParseCsvLine(inputFile.ReadLine()!).ToArray();
                    Array.Clear(newRow);
                    newRow[0] = GetConnectorFromPath(file);
                    for (int i = 0; i < row.Length; i++)
                    {
                        int? index = newColIndex[i];
                        if (index != null)
                        {
                            newRow[index.Value] = CleanString(row[i]);
                        }
                    }
                    mergeFileStream.WriteLine(string.Join(CsvSeparator, newRow));

                } while (!inputFile.EndOfStream);

                newColIndex.Clear();
                inputFile.Close();
                mergeFileStream.Flush();
            }
        }

        Console.WriteLine($"Merge file written: {mergeFilePath}");
    }

    private static string CleanString(string str) => str.Replace(",", "");

    private static IEnumerable<string> ParseCsvLine(string line, char separator = CsvSeparator)
    {
        bool inString = false;
        StringBuilder current = new StringBuilder();
        foreach (var c in line)
        {
            if (c == separator)
            {
                if (!inString)
                {
                    yield return current.ToString();
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            else if (c == '"')
            {
                inString = !inString;
                current.Append(c);
            }
            else
            {
                current.Append(c);
            }
        }
        yield return current.ToString();
    }

    private static bool CheckArgs(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("JoinBenchmarks needs 1 argument : a path where *-report.csv files are.");
            Console.WriteLine("Example: JoinBenchmarks c:\\test will merge all csv files in c:\\test directory");

            return false;
        }

        string path = args[0];
        if (!Directory.Exists(path))
        {
            Console.WriteLine("Path does not exists!");
            return false;
        }

        return true;
    }
}
