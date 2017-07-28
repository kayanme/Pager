using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using static Pager.PageManagerConfiguration;

namespace Benchmark.Pager
{
    public static class Program
    {
        public static void Main()
        {
            //var t = new RecordChangeBenchmark { WriteMethod = WriteMethod.VariableSize };
            //t.Init();
            //t.AddRecord();
            //t.DeleteFile();
            //t.Init();
            //t.AddRecord();
            //t.DeleteFile();
            BenchmarkDotNet.Running.BenchmarkRunner.Run<RecordAddBenchmark>(new C());
            Console.ReadKey();
        }
    }

    public class C:ManualConfig
    {
        public C()
        {
            this.Add(Job.Clr.WithInvocationCount(96*2).WithLaunchCount(3).WithAnalyzeLaunchVariance(true));
            Add(DefaultConfig.Instance.GetExporters().ToArray());
            Add(DefaultConfig.Instance.GetLoggers().ToArray());
            Add(DefaultConfig.Instance.GetColumnProviders().ToArray());

        }
    }
}
