using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using System.IO;
using System.Reflection;

namespace EDBBenchmark
{
    public class EDBManualConfig : ManualConfig
    {
        public EDBManualConfig()
        {
            var assemblyName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);            
            ArtifactsPath = $"./BenchmarkDotNet.Artifacts.{assemblyName}";

            AddColumn(StatisticColumn.OperationsPerSecond);
            AddColumn(new TagColumn("JobNameAgnostic", name => name.Replace("EDB","")));

            var summaryStyle = new SummaryStyle(cultureInfo: System.Globalization.CultureInfo.InvariantCulture, printUnitsInHeader: true, sizeUnit: null, timeUnit: Perfolizer.Horology.TimeUnit.Microsecond, printUnitsInContent: false);
            WithSummaryStyle(summaryStyle);
            WithOption(ConfigOptions.JoinSummary, true);

#if DEBUG
            //--IterationCount 5 --warmupCount 5 -f *BindVariable*
            AddJob(Job.Default.WithId("net9.0"));
#else
            switch (assemblyName)
            {
                case "BenchmarkVersion4":
                    AddJob(Job.Default.WithId("net8.0"));
                    break;
                case "BenchmarkNetStandard":
                    AddJob(Job.Default.WithId("net462"));
                    return; // only runtime is net462
            }

            AddJob(Job.Default.WithId("net8.0"));
            AddJob(Job.Default.WithId("net9.0"));
            AddJob(Job.Default.WithId("net10.0"));
            AddJob(Job.Default.WithRuntime(ClrRuntime.Net472).WithId("net472"));
            AddJob(Job.Default.WithRuntime(ClrRuntime.Net48).WithId("net48"));
            AddJob(Job.Default.WithRuntime(ClrRuntime.Net481).WithId("net481"));
#endif
        }
    }
}
