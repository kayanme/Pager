using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

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
            Directory.CreateDirectory("..\\Benchmarks\\");
            RunAndPrint<RecordAddBenchmark>("Add");
            RunAndPrint<Logical_RecordSearch>("Search");
          
          


        }

        private static void RunAndPrint<T>(string name)
        {
            var r = BenchmarkConverter.TypeToBenchmarks(typeof(T), new C());
            var ass = AppDomain.CurrentDomain.GetAssemblies().First(k => k.FullName.Contains("IO.Paging.PhysicalLevel"));
            var version = ass.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            var result = BenchmarkRunnerCore.Run(r, _ => new InProcessToolchain(false));
            foreach (var c in MarkdownExporter.GitHub.ExportToFiles(result, BenchmarkDotNet.Loggers.ConsoleLogger.Default))
            {
                File.Move(c, $"..\\Benchmarks\\{name}_{version}.md");
                
            }
            foreach (var c in HtmlExporter.Default.ExportToFiles(result, BenchmarkDotNet.Loggers.ConsoleLogger.Default))
            {
                File.Move(c, $"..\\Benchmarks\\{name}_{version}.html");
            }
            var exp = new BenchmarkDotNet.Exporters.Csv.CsvExporter(BenchmarkDotNet.Exporters.Csv.CsvSeparator.Semicolon);
            foreach (var c in exp.ExportToFiles(result, BenchmarkDotNet.Loggers.ConsoleLogger.Default))
            {
                File.Move(c, $"..\\Benchmarks\\{name}_{version}.csv");
            }
        }
    }
}
