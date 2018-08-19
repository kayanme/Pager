using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;
using System;
using System.Linq;

namespace Benchmark.TimeArchiver
{
    class Program
    {
        static void Main(string[] args)
        {
            var r = BenchmarkConverter.TypeToBenchmarks(typeof(TimeArchiver_PagesBenchmark), new C());          
            BenchmarkRunnerCore.Run(r, _ =>new InProcessToolchain(TimeSpan.FromHours(1),BenchmarkActionCodegen.ReflectionEmit,false));
            Console.ReadLine();
        }
    }

    public class C : ManualConfig
    {
        public C()
        {
            Add(Job.Core.WithInvocationCount(96 * 2).WithLaunchCount(3).WithAnalyzeLaunchVariance(true));
            Add(DefaultConfig.Instance.GetExporters().ToArray());
            Add(DefaultConfig.Instance.GetLoggers().ToArray());
            Add(DefaultConfig.Instance.GetColumnProviders().ToArray());

        }
    }
}
