using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.MemoryStubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TimeArchiver.Classes;
using TimeArchiver.Classes.Paging;
using TimeArchiver.Contracts;

namespace Test.TimeArchiver.DataInteractor
{
    [TestClass]
    public class DataInteractorTests
    {
        public TestContext TestContext { get; set; }

        private IDataPageInteractor<int> _interactor;
        private IPageManager _pm;

        [TestInitialize]
        public void Init()
        {
            _pm = new FactoryStub().CreateManager("", new DataPageConfiguration(), false);
            _interactor = new DataPageInteractor<int>(1, _pm);
        }

        [TestMethod]
        public void InsertPageWithNoDuplicates()
        {
            var records = new[]
            {
                new DataRecord<int>{Data = 2,Stamp = 2,VersionStamp = 0},
                new DataRecord<int>{Data = 3,Stamp = 3,VersionStamp = 1},
                new DataRecord<int>{Data = 1,Stamp = 1,VersionStamp = 2}
            };
            var dataRef = _interactor.CreateDataBlock(records);
            var head = _pm.GetHeaderAccessor<DataPageHeader>(dataRef.DataReference).GetHeader();
            Assert.AreEqual(1, head.StampOrigin);
            Assert.AreEqual(0, head.VersionOrigin);
            Assert.AreEqual(0,head.HasSameStampValues);
            using (var page = _pm.GetRecordAccessor<DataPageRecord<int>>(dataRef.DataReference))
            {
                var recs = page.IterateRecords().Select(k=>k.Data).ToArray();
                //в реальности порядок будет другой, это недоработка стаба
                Assert.AreEqual(0, recs[2].StampShift);
                Assert.AreEqual(2, recs[2].VersionShift);
                Assert.AreEqual(1, recs[2].Value);

                Assert.AreEqual(1, recs[0].StampShift);
                Assert.AreEqual(0, recs[0].VersionShift);
                Assert.AreEqual(2, recs[0].Value);

                Assert.AreEqual(2, recs[1].StampShift);
                Assert.AreEqual(1, recs[1].VersionShift);
                Assert.AreEqual(3, recs[1].Value);

                
            }
        }


        [TestMethod]
        public void InsertPageWithDuplicates()
        {
            var records = new[]
            {
                new DataRecord<int>{Data = 2,Stamp = 2,VersionStamp = 0},
                new DataRecord<int>{Data = 3,Stamp = 2,VersionStamp = 1},
                new DataRecord<int>{Data = 1,Stamp = 1,VersionStamp = 2}
            };
            var dataRef = _interactor.CreateDataBlock(records);
            var head = _pm.GetHeaderAccessor<DataPageHeader>(dataRef.DataReference).GetHeader();
            Assert.AreEqual(1, head.StampOrigin);
            Assert.AreEqual(0, head.VersionOrigin);
            Assert.AreEqual(1, head.HasSameStampValues);
            using (var page = _pm.GetRecordAccessor<DataPageRecord<int>>(dataRef.DataReference))
            {
                var recs = page.IterateRecords().Select(k => k.Data).ToArray();
                Assert.AreEqual(0, recs[2].StampShift);
                Assert.AreEqual(2, recs[2].VersionShift);
                Assert.AreEqual(1, recs[2].Value);

                Assert.AreEqual(1, recs[0].StampShift);
                Assert.AreEqual(0, recs[0].VersionShift);
                Assert.AreEqual(2, recs[0].Value);

                Assert.AreEqual(1, recs[1].StampShift);
                Assert.AreEqual(1, recs[1].VersionShift);
                Assert.AreEqual(3, recs[1].Value);

            }
        }
    }
}
