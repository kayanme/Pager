using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benchmark.Pager;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Benchmark.Paging.LogicalLevel
{
    class Program
    {
        static void Main(string[] args)
        {
            //var t = new RecordAddBenchmark();
            //t.Init();
            //t.ChangeRecordWithOrder();
            //foreach (var t2 in Enumerable.Range(0, 100))
            //    t.AddRecordWithOrder();
            //t.AddRecordWithOrder();
            //t.AddRecordWithOrder();
            //t.DeleteFile();
                BenchmarkDotNet.Running.BenchmarkRunner.Run<RecordAddBenchmark>(new C());
               Console.ReadKey();
        }
    }

    public class C : ManualConfig
    {
        public C()
        {
            this.Add(Job.Clr.WithInvocationCount(96).WithLaunchCount(3).WithAnalyzeLaunchVariance(true));
            Add(DefaultConfig.Instance.GetExporters().ToArray());
            Add(DefaultConfig.Instance.GetLoggers().ToArray());
            Add(DefaultConfig.Instance.GetColumnProviders().ToArray());

        }
    }
}
