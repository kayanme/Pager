using System;
using FakeItEasy;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Contracts.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Test.Pager.Pages
{
    [TestClass]
    public class FixedSizePageTest
    {
        public TestContext TestContext { get; set; }
        private IPage<TestRecord> Create()
        {
          
            var headers = A.Fake<IPageHeaders>();
            var access = A.Fake<IRecordAcquirer<TestRecord>>();
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
            A.CallTo(()=> Headers.TakeNewRecord(0, 4)).Returns<short>(0);
            A.CallTo(() => Headers.RecordShift(0)).Returns<ushort>(2);
            A.CallTo(() => Headers.RecordSize(0)).Returns<ushort>(4);
            var r = new TestRecord{Value = 2};
            
           
            var res = page.AddRecord(r);
            Assert.AreEqual(r,res.Data);
            Assert.AreEqual(0, res.Reference.PersistentRecordNum);
            A.CallTo(() => Access.SetRecord(2, 4, r)).MustHaveHappened();
        }

        [TestMethod]
        public void AddRecord_NoSpace()
        {
            var page = Create();
            A.CallTo(() => Headers.TakeNewRecord(0, 4)).Returns<short>(-1);
            var r = new TestRecord { Value = 2 };
           
           

            var res = page.AddRecord(r);
            Assert.IsNull(res);            
        }

    }
}
