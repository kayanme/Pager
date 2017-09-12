using System;
using System.Linq;
using File.Paging.PhysicalLevel.Classes.Configurations;

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
                      BenchmarkDotNet.Running.BenchmarkRunner.Run<Logical_RecordSearch>(new C());
            //        BenchmarkDotNet.Running.BenchmarkRunner.Run<RecordAddBenchmark>(new C());

            //    Console.ReadKey();
        }
    }
}
