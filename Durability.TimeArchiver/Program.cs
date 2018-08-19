using FIle.Paging.LogicalLevel.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeArchiver.Classes.Paging;
using TimeArchiver.Contracts;

namespace Test.Integration.TimeArchiver
{
    class Program
    {
        private const int _tagNum = 10;

        static void Main(string[] args)
        {
            var pmf = new LogicalPageManagerFactory();
            var searcher = new DataSearch("roots","index1","index2","data",pmf);
            Task.WaitAll(Enumerable.Range(0, _tagNum).Select(k=> searcher.CreateTag(k, TagType.Int)).ToArray());

            var writers =  new[] { WriteDataFull(searcher) };
            var readers = Enumerable.Range(0, 10).Select(_ => ReadDataFull(searcher)).ToArray();

            Task.WaitAll(writers.Concat(readers).ToArray());

        }

        private static async Task ReadData(IDataSearch ds)
        {
            var rnd = new Random();
            var st = rnd.Next(1);
            var en = rnd.Next(100);
            var tag = rnd.Next(_tagNum);
            var res =  ds.FindInRangeInt(tag, st, en).ToEnumerable().SelectMany(k=>k).ToArray();
            Console.WriteLine($"found {res.Count()} numbers in {tag}  from {st} to {en} ");
            await Task.Delay(3);
        }
        private static async Task WriteDataFull(IDataSearch ds)
        {
            foreach(var _ in Enumerable.Range(0, 5000))
            {
                await WriteData(ds);
            }
        }

        private static async Task ReadDataFull(IDataSearch ds)
        {
            foreach (var _ in Enumerable.Range(0, 50000))
            {
                await ReadData(ds);
            }
        }
        private static async Task WriteData(IDataSearch ds)
        {
            var rnd = new Random();
            var st = rnd.Next(1);
            var en = rnd.Next(10000);
            var tag = rnd.Next(_tagNum);
            IEnumerable<int> RandomInf(int low,int hi)
            {
                while (true) yield return rnd.Next(low,hi);
            }

            var times = RandomInf(st, en).Take(DataPageRecord<int>.MaxRecordsOnPage - 1).ToArray().OrderBy(k => k).ToArray();
            var versions = RandomInf(st, en).Take(DataPageRecord<int>.MaxRecordsOnPage - 1).ToArray();
            var vals = times.Zip(versions, (t, v) => new DataRecord<int> { Data = t, VersionStamp = v, Stamp = t }).ToArray();

            await ds.InsertBlock(tag,vals);
            Console.WriteLine($"insert {vals.Count()} into {tag} numbers from {st} to {en} ");
            await Task.Delay(3);
        }
    }
}
