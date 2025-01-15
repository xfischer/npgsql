using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System;

namespace EDBBenchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
            Console.ReadLine();
#else
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
#endif
        }
    }
}
