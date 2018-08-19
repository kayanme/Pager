using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using File.Paging.PhysicalLevel.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Paging.PhysicalLevel.Headers
{



    [TestClass]
    public class PageHeadersBaseTest
    {
        internal abstract class TestHeaders : PageHeadersBase
        {
            protected TestHeaders(int maxRecordCount) : base()
            {
                RecordInfo = new int[maxRecordCount];
            }

            public abstract IEnumerable<Tuple<int, int, int>> FillHeadersMock();

            protected sealed override int[] RecordInfo { get; }

            public void FillHeaders()
            {
                var t = FillHeadersMock();
                var d = t.Select(k => FormRecordInf((byte)k.Item3, (ushort)k.Item2, (ushort)k.Item1)).ToArray();
                TotalUsedRecords = (ushort)t.Count(k => k.Item1 != 0);
                for (int i = 0; i < RecordInfo.Length; i++)
                {
                    RecordInfo[i] = d[i];
                }
            }

            protected sealed override int HeaderOverheadSize => 0;

            protected sealed override IEnumerable<int> PossibleRecordsToInsert()
            {
                return Enumerable.Range(0, RecordInfo.Length);
            }

            protected sealed override void SetFree(ushort record)
            {
                TestSetFree(record);
            }

            protected sealed override ushort SetUsed(ushort record, ushort size, byte type)
            {
                return TestSetUsed(record, size, type);
            }

            public abstract void TestSetFree(ushort record);

            public abstract ushort TestSetUsed(ushort record, ushort size, byte type);
        }

        private TestHeaders CreateHeaders(int maxRecords)
        {

            var headers = A.Fake<TestHeaders>(o => o.WithArgumentsForConstructor(new object[] { maxRecords }));
            A.CallTo(() => headers.RecordShift(0)).WithAnyArguments().CallsBaseMethod();
            return headers;
        }

        [TestMethod]
        public void MainRecordParameters()
        {
            var headers = CreateHeaders(1);
            A.CallTo(() => headers.FillHeadersMock())
                               .Returns(new[]
                               {
                       Tuple.Create(10,40,14)
                               });

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
            A.CallTo(() => headers.FillHeadersMock())
                               .Returns(new[]
                               {
                       Tuple.Create(10,40,14)
                               });

            headers.FillHeaders();

            Assert.IsFalse(headers.IsRecordFree(0));
        }

        [TestMethod]
        public void CheckFreeRecordForFree()
        {
            var headers = CreateHeaders(1);
            A.CallTo(() => headers.FillHeadersMock())
                               .Returns(new[]
                               {
                       Tuple.Create(0,0,0)
                               });

            headers.FillHeaders();

            Assert.IsTrue(headers.IsRecordFree(0));
        }

        [TestMethod]
        public void FreeRecord()
        {
            var headers = CreateHeaders(1);
            A.CallTo(() => headers.FillHeadersMock())
                               .Returns(new[]
                               {
                                   Tuple.Create(10,10,2)
                               });
            

            headers.FillHeaders();

            headers.FreeRecord(0);
            A.CallTo(() => headers.TestSetFree(0)).MustHaveHappened();
            Assert.AreEqual(0, headers.RecordCount);
            Assert.AreEqual(10, headers.RecordShift(0));
            Assert.AreEqual(10, headers.RecordSize(0));
            Assert.AreEqual(0, headers.RecordType(0));
        }

        [TestMethod]
        public void UseRecord()
        {
            var headers = CreateHeaders(1);
            A.CallTo(() => headers.FillHeadersMock())
                               .Returns(new[]
                               {
                       Tuple.Create(0,0,0)
                               });
            A.CallTo(() => headers.TestSetUsed(0, 30, 3)).Returns<ushort>(2);

            headers.FillHeaders();

            var recordNum = headers.TakeNewRecord(3, 30);

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
            A.CallTo(() => headers.FillHeadersMock())
                               .Returns(new[]
                               {
                       Tuple.Create(10,40,2)
                               });

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
