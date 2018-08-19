using FIle.Paging.LogicalLevel.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeArchiver.Classes.Paging;
using TimeArchiver.Contracts;

namespace Test.Integration.TimeArchiver
{
    [TestClass]
    public class SomeReadsAndWritesToArchiver
    {
        [TestMethod]
        [Timeout(TestTimeout.Infinite)]
        public async Task ReadWriteToArchiver()
        {
            var pmf = new LogicalPageManagerFactory();
            using (var searcher = new DataSearch("roots", "index1", "index2", "data", pmf))
            {
                await searcher.CreateTag(1, TagType.Int);
                var rnd = new Random();

                async Task InsertBlock(int shift)
                {
                    var vals = Enumerable.Range(0 + shift, DataPageRecord<int>.MaxRecordsOnPage)
                   .Select(k => new DataRecord<int> { Data = k, VersionStamp = k, Stamp = k }).ToArray();

                    await searcher.InsertBlock(1, vals);
                }
                foreach (var t in Enumerable.Range(0, 500).Select(k => k * DataPageRecord<int>.MaxRecordsOnPage))
                {
                    await InsertBlock(t);
                }


                var res = searcher.FindInRangeInt(1, 201, 500).ToEnumerable().ToArray();
                Assert.IsTrue(res.SelectMany(k => k).All(k => k.Data == k.Stamp));
                Assert.AreEqual(300, res.SelectMany(k => k).Count());

                res = searcher.FindInRangeInt(1, 2001, 5000).ToEnumerable().ToArray();
                Assert.IsTrue(res.SelectMany(k => k).All(k => k.Data == k.Stamp));
                Assert.AreEqual(3000, res.SelectMany(k => k).Count());
            }
        }


        [TestCleanup]
        public void TestCleanup()
        {
            System.IO.File.Delete("roots");
            System.IO.File.Delete("index1");
            System.IO.File.Delete("index2");
            System.IO.File.Delete("data");
        }
    }
}
