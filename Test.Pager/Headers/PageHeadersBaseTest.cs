using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pager.Contracts;
using Pager.Implementations;
using Rhino.Mocks;

namespace Test.Pager
{



    [TestClass]
    public class PageHeadersBaseTest
    {
        internal abstract class TestHeaders : PageHeadersBase
        {
            protected TestHeaders(int maxRecordCount) : base()
            {
                _recordInfo = new int[maxRecordCount];
            }

            public abstract IEnumerable<Tuple<int, int, int>>  FillHeadersMock();

            protected sealed override int[] _recordInfo { get; }

            public void FillHeaders()
            {
                var d = FillHeadersMock().Select(k => FormRecordInf((byte)k.Item3, (ushort)k.Item2, (ushort)k.Item1)).ToArray();

                for (int i=0;i<_recordInfo.Length;i++)
                {
                    _recordInfo[i] = d[i];
                }
            }

            protected sealed override IEnumerable<int> PossibleRecordsToInsert()
            {
                return Enumerable.Range(0, _recordInfo.Length);
            }

            protected sealed override void SetFree(ushort record)
            {
                TestSetFree(record);
            }

            protected sealed override ushort SetUsed(ushort record, ushort size, byte type)
            {
               return TestSetUsed(record,size,type);
            }

            public abstract void TestSetFree(ushort record);

            public abstract ushort TestSetUsed(ushort record, ushort size, byte type);
        }

        private TestHeaders CreateHeaders(int maxRecords)
        {
            var m = new MockRepository();
            var headers = m.StrictMock<TestHeaders>(maxRecords);
            headers.Expect(k => k.RecordShift(0)).IgnoreArguments().CallOriginalMethod(Rhino.Mocks.Interfaces.OriginalCallOptions.NoExpectation);
            return headers;
        }

        [TestMethod]
        public void MainRecordParameters()
        {
            var headers = CreateHeaders(1);
            headers.Expect(k => k.FillHeadersMock())                  
                   .Return(new[]
                   {
                       Tuple.Create(10,40,14)
                   });
            headers.Replay();
            headers.FillHeaders();

            Assert.AreEqual(1, headers.RecordCount);
            Assert.AreEqual(10, headers.RecordShift(0));
            Assert.AreEqual(40, headers.RecordSize(0));
            Assert.AreEqual(14, headers.RecordType(0));
        }

        [TestMethod]
        public void CheckNonFreeRecordForFree()
        {
            var headers = CreateHeaders(1);
            headers.Expect(k => k.FillHeadersMock())
                   .Return(new[]
                   {
                       Tuple.Create(10,40,14)
                   });
            headers.Replay();
            headers.FillHeaders();

            Assert.IsFalse(headers.IsRecordFree(0));
        }

        [TestMethod]
        public void CheckFreeRecordForFree()
        {
            var headers = CreateHeaders(1);
            headers.Expect(k => k.FillHeadersMock())
                   .Return(new[]
                   {
                       Tuple.Create(0,0,0)
                   });
            headers.Replay();
            headers.FillHeaders();

            Assert.IsTrue(headers.IsRecordFree(0));
        }

        [TestMethod]
        public void FreeRecord()
        {
            var headers = CreateHeaders(1);
            headers.Expect(k => k.FillHeadersMock())
                   .Return(new[]
                   {
                       Tuple.Create(10,10,2)
                   });
            headers.Expect(k => k.TestSetFree(0));
            headers.Replay();
            headers.FillHeaders();
        
            headers.FreeRecord(0);

            Assert.AreEqual(0, headers.RecordCount);
            Assert.AreEqual(0, headers.RecordShift(0));
            Assert.AreEqual(0, headers.RecordSize(0));
            Assert.AreEqual(0, headers.RecordType(0));
        }

        [TestMethod]
        public void UseRecord()
        {
            var headers = CreateHeaders(1);
            headers.Expect(k => k.FillHeadersMock())
                   .Return(new[]
                   {
                       Tuple.Create(0,0,0)
                   });
            headers.Expect(k => k.TestSetUsed(0, 30, 3)).Return(2);
            headers.Replay();
            headers.FillHeaders();
           
            var recordNum = headers.TakeNewRecord(3,30);

            Assert.AreEqual(0, recordNum);
            Assert.AreEqual(1, headers.RecordCount);
            Assert.AreEqual(2, headers.RecordShift(0));
            Assert.AreEqual(30, headers.RecordSize(0));
            Assert.AreEqual(3, headers.RecordType(0));

        }

        [TestMethod]
        public void UseRecord_WhenThereIsNoFree()
        {
            var headers = CreateHeaders(1);
            headers.Expect(k => k.FillHeadersMock())
                   .Return(new[]
                   {
                       Tuple.Create(10,40,2)
                   });
            headers.Replay();
            headers.FillHeaders();         
            var recordNum = headers.TakeNewRecord(3, 30);

            Assert.AreEqual(-1, recordNum);
            Assert.AreEqual(1, headers.RecordCount);
            Assert.AreEqual(10, headers.RecordShift(0));
            Assert.AreEqual(40, headers.RecordSize(0));
            Assert.AreEqual(2, headers.RecordType(0));

        }
    }
}
