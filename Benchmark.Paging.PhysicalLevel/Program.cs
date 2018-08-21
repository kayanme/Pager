using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;


namespace Benchmark.Paging.PhysicalLevel
{
    public static class Program
    {
        public static void Main()
        {
            //var t = new Physical_RecordAddBenchmark();
            //t.Init();
            //t.AddRecordWithFlush();

            BenchmarkRunInfo r;
            r = BenchmarkConverter.TypeToBenchmarks(typeof(Physical_RecordSearchBenchmark), new C("Search"));
            BenchmarkRunnerCore.Run(r, _ => new InProcessToolchain(false));
            r = BenchmarkConverter.TypeToBenchmarks(typeof(Physical_RecordAddBenchmark), new C("Add"));
            BenchmarkRunnerCore.Run(r, _ => new InProcessToolchain(false));
            r = BenchmarkConverter.TypeToBenchmarks(typeof(Physical_RecordChangeBenchmark), new C("Change"));
            BenchmarkRunnerCore.Run(r, _ => new InProcessToolchain(false));
            r = BenchmarkConverter.TypeToBenchmarks(typeof(Physical_RecordIterateBenchmark), new C("Iterate"));
            BenchmarkRunnerCore.Run(r, _ => new InProcessToolchain(false));
            r = BenchmarkConverter.TypeToBenchmarks(typeof(Physical_LockBenchmark), new C("Lock"));
            BenchmarkRunnerCore.Run(r, _ => new InProcessToolchain(false));
        }
    }
}
