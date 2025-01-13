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

    private static string GetConnectorFromPath(string file)
    {
        var dir = Directory.GetParent(Path.GetDirectoryName(file)!)!.Name;
        return dir switch
        {
            "BenchmarkDotNet.Artifacts.BenchmarkVersion7" => "7.0.6.2",
            "BenchmarkDotNet.Artifacts.BenchmarkVersion8" => "8.0.5.2",
            "BenchmarkDotNet.Artifacts.BenchmarkVersion9" => "9.0.2.1",
            "BenchmarkDotNet.Artifacts.BenchmarkVersion4" => "4.0.10.2",
            "BenchmarkDotNet.Artifacts.BenchmarkNpgsql" => "Npgsql8",
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
            var currentHeader = inputFile.ReadLine().Split(CsvSeparator);
            inputFile.Close();

            foreach (var colName in currentHeader)
            {
                if (!cumulativeHeaders.ContainsKey(colName))
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

            Dictionary<int, int> newColIndex = new Dictionary<int, int>();

            foreach (var file in Directory.EnumerateFiles(path, FilePattern, SearchOption.AllDirectories))
            {
                Console.WriteLine($"Processing file {Path.GetFileName(file)}");
                using var inputFile = new StreamReader(file, encoding: Encoding.UTF8, false, new FileStreamOptions() { Access = FileAccess.Read, Mode = FileMode.Open, Share = FileShare.Read });
                var currentHeader = inputFile.ReadLine().Split(CsvSeparator);

                for (int i = 0; i < currentHeader.Length; i++)
                {
                    newColIndex[i] = cumulativeHeaders[currentHeader[i]];
                }


                string[] newRow = new string[cumulativeHeaders.Count];
                do
                {
                    var row = ParseCsvLine(inputFile.ReadLine()).ToArray();
                    Array.Clear(newRow);
                    newRow[0] = GetConnectorFromPath(file);
                    for (int i = 0; i < row.Length; i++)
                    {
                        newRow[newColIndex[i]] = row[i];
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

    private static void JoinOneFilePerHeader(string path)
    {
        Dictionary<string, StreamWriter> fileByHeader = new();

        foreach (var file in Directory.EnumerateFiles(path, FilePattern, SearchOption.AllDirectories))
        {
            var currentConnector = GetConnectorFromPath(file);
            bool firstLine = true;

            Console.WriteLine($"Processing file {file}");

            string currentHeaderKey = null;
            using var inputFile = new StreamReader(file, encoding: Encoding.UTF8, false, new FileStreamOptions() { Access = FileAccess.Read, Mode = FileMode.Open, Share = FileShare.Read });
            do
            {
                StreamWriter outStream;
                if (firstLine)
                {
                    currentHeaderKey = inputFile.ReadLine();
                    if (!fileByHeader.TryGetValue(currentHeaderKey, out outStream))
                    {
                        string mergeFilePath = Path.Combine(path, $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}-mergedreport-{fileByHeader.Count}.csv");
                        outStream = new StreamWriter(mergeFilePath, append: false, Encoding.UTF8);
                        fileByHeader[currentHeaderKey] = outStream;
                        outStream.Write("Connector" + CsvSeparator);
                        outStream.WriteLine(currentHeaderKey);
                    }

                    firstLine = false;
                }
                else
                {
                    outStream = fileByHeader[currentHeaderKey];
                    outStream.Write($"{currentConnector},");

                    outStream.WriteLine(inputFile.ReadLine());
                }

            } while (!inputFile.EndOfStream);
        }

        foreach (var file in fileByHeader.Values)
        {
            file.Flush();
            file.Close();
            file.Dispose();
        }
    }

    private static void JoinWithSameHeader(string path)
    {
        string mergeFilePath = Path.Combine(path, $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}-mergedreport.csv");
        using (var mergeFileStream = new StreamWriter(mergeFilePath, append: false, Encoding.UTF8))
        {
            bool firstFile = true;
            foreach (var file in Directory.EnumerateFiles(path, FilePattern, SearchOption.AllDirectories))
            {
                bool firstLine = true;
                var currentConnector = GetConnectorFromPath(file);

                Console.WriteLine($"Processing file {file}");

                var indexes = new List<int>();
                using var inputFile = new StreamReader(file, encoding: Encoding.UTF8, false, new FileStreamOptions() { Access = FileAccess.Read, Mode = FileMode.Open, Share = FileShare.Read });
                do
                {
                    List<string> headerColumns;
                    if (firstLine)
                    {
                        var rawHeader = inputFile.ReadLine();
                        headerColumns = rawHeader!.Split(CsvSeparator).ToList();
                        for (int i = 0; i < columnsToKeep!.Length; i++)
                        {
                            int indexResult = headerColumns.IndexOf(columnsToKeep[i]);
                            if (indexResult > -1) indexes.Add(indexResult);
                        }

                        if (firstFile)
                        {
                            mergeFileStream.Write("Connector" + CsvSeparator);
                            mergeFileStream.WriteLine(string.Join(CsvSeparator, columnsToKeep));
                            firstFile = false;
                        }
                        firstLine = false;
                    }
                    else
                    {
                        mergeFileStream.Write($"{currentConnector},");

                        var values = ParseCsvLine(inputFile.ReadLine()!).ToList();
                        foreach (var index in indexes)
                        {
                            if (index == indexes[^1])
                            {
                                mergeFileStream.WriteLine(values[index]);
                            }
                            else
                            {
                                mergeFileStream.Write($"{values[index]},");
                            }
                        }
                    }

                } while (!inputFile.EndOfStream);
            }
        }

        Console.WriteLine($"Merge file written: {mergeFilePath}");

    }

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
