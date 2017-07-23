using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using static Pager.PageMapConfiguration;

namespace Benchmark.Pager
{
    public static class Program
    {
        public static void Main()
        {
            //var t = new RecordChangeBenchmark { PageSize = PageSize.Kb4 };
            //t.Init();
            //t.AddRecord();
            //t.DeleteFile();
            //t.Init();
            //t.AddRecord();
            //t.DeleteFile();
            BenchmarkDotNet.Running.BenchmarkRunner.Run<RecordChangeBenchmark>(new C());
            Console.ReadKey();
        }
    }

    public class C:ManualConfig
    {
        public C()
        {
            this.Add(Job.Clr.WithInvocationCount(48));
            Add(DefaultConfig.Instance.GetExporters().ToArray());
            Add(DefaultConfig.Instance.GetLoggers().ToArray());
            Add(DefaultConfig.Instance.GetColumnProviders().ToArray());

        }
    }
}
