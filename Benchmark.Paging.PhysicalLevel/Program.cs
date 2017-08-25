using System;
using System.Linq;
using Benchmark.Paging.PhysicalLevel;
using BenchmarkDotNet.Running;


namespace Benchmark.Paging.PhysicalLevel
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
            //      BenchmarkRunner.Run<Physical_RecordAddBenchmark>(new C());
            //      BenchmarkRunner.Run<Physical_RecordChangeBenchmark>(new C());
            //        BenchmarkRunner.Run<Physical_LockBenchmark>(new C());
            //  BenchmarkRunner.Run<SearchBench>();
            var t = new Physical_RecordAddBenchmark{WriteMethod = WriteMethod.FixedSizeWithOrder};
            t.Init();
            foreach (var _ in Enumerable.Repeat(0, 1000000))
            {
                 t.AddRecord();
            }
          //  Console.ReadKey();
        }
    }
}
