﻿using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Benchmark.Paging.PhysicalLevel
{
    public static class Program
    {
        public static void Main()
        {
            //var t = new Physical_RecordAddBenchmark();
            //t.Init();
            //t.AddRecordWithFlush();     
            var dir = Directory.CreateDirectory("Benchmarks//Physical");
        //    RunAndPrint< Physical_RecordSearchBenchmark>("Search");
            RunAndPrint<Physical_RecordAddBenchmark>("Add");
         //   RunAndPrint<Physical_RecordChangeBenchmark>("Change");
          //  RunAndPrint<Physical_RecordIterateBenchmark>("Iterate");
          //  RunAndPrint<Physical_LockBenchmark>("Lock");
            foreach (var f in dir.EnumerateFiles())
            {
                ConsoleLogger.Default.WriteLine(f.FullName);
            }
        }


        private static void RunAndPrint<T>(string name)
        {
            var r = BenchmarkConverter.TypeToBenchmarks(typeof(T), new C(name));
            var ass = AppDomain.CurrentDomain.GetAssemblies().First(k => k.FullName.Contains("IO.Paging.PhysicalLevel"));
            var version = ass.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            var result = BenchmarkRunnerCore.Run(r, _ => new InProcessToolchain(false));
            foreach (var c in MarkdownExporter.GitHub.ExportToFiles(result, BenchmarkDotNet.Loggers.ConsoleLogger.Default))
            {
                var path = Path.GetFullPath($"Benchmarks//Physical//{name}_{version}.md");
                System.IO.File.Move(c, path);
                ConsoleLogger.Default.WriteLine($"results at {path}");
            }
            foreach (var c in HtmlExporter.Default.ExportToFiles(result, BenchmarkDotNet.Loggers.ConsoleLogger.Default))
            {
                var path = Path.GetFullPath($"Benchmarks//Physical//{name}_{version}.html");
                System.IO.File.Move(c, path);
                ConsoleLogger.Default.WriteLine($"results at {path}");
            }
            var exp = new BenchmarkDotNet.Exporters.Csv.CsvExporter(BenchmarkDotNet.Exporters.Csv.CsvSeparator.Semicolon);
            foreach (var c in exp.ExportToFiles(result, BenchmarkDotNet.Loggers.ConsoleLogger.Default))
            {
                var path = Path.GetFullPath($"Benchmarks//Physical//{name}_{version}.csv");
               System.IO.File.Move(c, path);
                ConsoleLogger.Default.WriteLine($"results at {path}");
            }
          

        }


    }
}
