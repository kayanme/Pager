using System;

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
            BenchmarkDotNet.Running.BenchmarkRunner.Run<Physical_RecordAddBenchmark>(new C());
            BenchmarkDotNet.Running.BenchmarkRunner.Run<Physical_RecordChangeBenchmark>(new C());
            Console.ReadKey();
        }
    }
}
