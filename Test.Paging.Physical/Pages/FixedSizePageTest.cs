using System;
using System.Runtime.InteropServices.ComTypes;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Contracts.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;

namespace Test.Pager.Pages
{
   [TestClass]
    public class FixedSizePageTest
    {
        public TestContext TestContext { get; set; }
        private IPage<TestRecord> Create()
        {
            var mp = new MockRepository();
            var headers = mp.StrictMock<IPageHeaders>();
            var access = mp.StrictMock<IRecordAcquirer<TestRecord>>();
            var config = new FixedRecordTypePageConfiguration<TestRecord>{PageSize = 4096,RecordMap = new FixedSizeRecordDeclaration<TestRecord>(null,null,4)};
            var page = new FixedRecordTypedPage<TestRecord>(headers, access, new PageReference(0), config, ()=>{});
            TestContext.Properties.Add("headers", headers);
            TestContext.Properties.Add("access", access);
            return page;
        }

        private IPageHeaders Headers => TestContext.Properties["headers"] as IPageHeaders;
        private IRecordAcquirer<TestRecord> Access => TestContext.Properties["access"] as IRecordAcquirer<TestRecord>;

        private Action<int, int, ByteAction> VerifyForArray(byte[] arr)
        {
            unsafe
            {
                return
                    (_, s, d) =>
                    {
                     
                        var t = new byte[s];
                        fixed (byte* t2 = t)
                        {
                            d(t2);
                        }
                        CollectionAssert.AreEqual(arr,t);
                    };
            }
        }

        [TestMethod]
        public unsafe void AddRecord()
        {
            var page = Create();
            Headers.Expect(k => k.TakeNewRecord(0, 4)).Return(0);
            Headers.Expect(k => k.RecordShift(0)).Return(2).Repeat.Any();
            Headers.Expect(k => k.RecordSize(0)).Return(4);
            var r = new TestRecord{Value = 2};
            Access.Expect(k=>k.SetRecord(2,4,r));
            Headers.Replay();
            Access.Replay();
            var res = page.AddRecord(r);
            Assert.AreEqual(r,res.Data);
            Assert.AreEqual(0, res.Reference.PersistentRecordNum);
            Headers.VerifyAllExpectations();
            Access.VerifyAllExpectations();
        }

        [TestMethod]
        public void AddRecord_NoSpace()
        {
            var page = Create();
            Headers.Expect(k => k.TakeNewRecord(0, 4)).Return(-1);
            var r = new TestRecord { Value = 2 };
           
            Headers.Replay();
            Access.Replay();

            var res = page.AddRecord(r);
            Assert.IsNull(res);

            Headers.VerifyAllExpectations();
            Access.VerifyAllExpectations();
        }

    }
}
