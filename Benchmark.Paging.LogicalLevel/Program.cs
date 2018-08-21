using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;
using System;

namespace Benchmark.Paging.LogicalLevel
{
    class Program
    {
        static void Main(string[] args)
        {
            //var t = new Logical_RecordSearch{PageSize = PageManagerConfiguration.PageSize.Kb4};
            //t.Init();
            //foreach (var i in Enumerable.Range(0,1000))
            //{
            //    t.Search();
            //    t.Scan();
            //}
           
           
            //t.AddRecordInVirtualPage();
            //foreach (var t2 in Enumerable.Range(0, 100000))
            //    t.AddRecordInVirtualPage();
            ////t.AddRecordWithOrder();
            ////t.AddRecordWithOrder();
            ////t.AddRecordWithOrder();
            //t.DeleteFile();
            var br = BenchmarkConverter.TypeToBenchmarks(typeof(RecordAddBenchmark),new C());
            BenchmarkDotNet.Running.BenchmarkRunnerCore.Run(br, _ => new InProcessToolchain(true));
            var lr = BenchmarkConverter.TypeToBenchmarks(typeof(Logical_RecordSearch), new C());
            BenchmarkDotNet.Running.BenchmarkRunnerCore.Run(lr, _ => new InProcessToolchain(true));
            //        BenchmarkDotNet.Running.BenchmarkRunner.Run<RecordAddBenchmark>(new C());

            
        }
    }
}
