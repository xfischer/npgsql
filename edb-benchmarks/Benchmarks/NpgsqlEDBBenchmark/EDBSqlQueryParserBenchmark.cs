using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using EnterpriseDB.EDBClient;

namespace EDBBenchmark;

[MemoryDiagnoser]
[Config(typeof(EDBManualConfig))]
public class EDBSqlQueryParserBenchmark
{
    List<string> queries = new();

    [GlobalSetup]
    public void Setup()
    {
        // Read all the queries from the sample file
        using StreamReader reader = new StreamReader("sql_log.txt");
        var sb = new StringBuilder();
        do
        {
            var line = reader.ReadLine();
            if (line == "### SQL QUERY PARSER LOG ###")
            {
                if (sb.Length > 0)
                {
                    queries.Add(sb.ToString());
                    sb.Clear();
                }
            }
            else
            {
                sb.AppendLine(line);
            }
        }
        while (!reader.EndOfStream);
    }

    [Benchmark]
    [BenchmarkCategory("EDBQueryParser")]
    public void ParseUsingSpans()
    {
        char[] trim = [' ', '\r', '\n'];
        var trimSpan = trim.AsSpan();
        foreach (var query in queries)
        {
            var parser = new SqlQueryParser(supportsRedwoodDialect: true);
            var sql = query.Trim(trim);
            
            parser.ContainsSPLStartingKeyword(sql);
        }
    }

}