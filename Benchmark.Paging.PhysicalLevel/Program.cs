using System;

namespace Benchmark.Paging.PhysicalLevel
{
    public static class Program
    {
        public static void Main()
        {
            var t = new RecordChangeBenchmark { WriteMethod = WriteMethod.VariableSize };
            t.Init();
            t.AddRecord();
            t.DeleteFile();
            //t.Init();
            //t.AddRecord();
            //t.DeleteFile();
            //   BenchmarkDotNet.Running.BenchmarkRunner.Run<RecordAddBenchmark>(new C());
            Console.ReadKey();
        }
    }
}
