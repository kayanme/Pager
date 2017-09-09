using System;
using System.Linq;
using Benchmark.Paging.PhysicalLevel;
using BenchmarkDotNet.Running;
using File.Paging.PhysicalLevel.Classes.Configurations;


namespace Benchmark.Paging.PhysicalLevel
{
    public static class Program
    {
        public static void Main()
        {

            //var t = new Physical_RecordSearchBenchmark() {PageSize = PageManagerConfiguration.PageSize.Kb4, WriteMethod = WriteMethod.FixedSize };
            //t.Init();
            //t.ScanSearch();
            //t.BinarySearch();

            //t.IteratePage();
            //t.IteratePage();
            //t.IteratePage();
            //t.Init();
            //t.AddRecord();
            //t.DeleteFile();
            BenchmarkRunner.Run<Physical_RecordSearchBenchmark>(new C());
         //   BenchmarkRunner.Run<Physical_RecordIterateBenchmark>(new C());
            //BenchmarkRunner.Run<Physical_RecordAddBenchmark>(new C());
            //BenchmarkRunner.Run<Physical_RecordChangeBenchmark>(new C());
            //BenchmarkRunner.Run<Physical_LockBenchmark>(new C());
            //  BenchmarkRunner.Run<SearchBench>();
            //var t = new Physical_RecordAddBenchmark { WriteMethod = WriteMethod.FixedSize };
            //t.Init();
            //foreach (var _ in Enumerable.Repeat(0, 1000000))
            //{
            //    t.AddRecord();
            //}
            //Console.ReadKey();
        }
    }
}
