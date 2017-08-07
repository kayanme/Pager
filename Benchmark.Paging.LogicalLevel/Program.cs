using System;

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
}
