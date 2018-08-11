using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.MemoryStubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeArchiver.Contracts;

namespace Test.TimeArchiver.IndexInteraction
{
    [TestClass]
    public class IndexInteractionTests
    {
        public TestContext TestContext { get; set; }

        private IPage<IndexPageRecord> _page1 { get => TestContext.Properties["p1"] as IPage<IndexPageRecord>; set { TestContext.Properties["p1"] = value; } }
        private IPage<IndexPageRecord> _page2 { get => TestContext.Properties["p2"] as IPage<IndexPageRecord>; set { TestContext.Properties["p2"] = value; } }
        private IIndexCorrection _indexInteraction { get => TestContext.Properties["ii"] as IIndexCorrection; set { TestContext.Properties["ii"] = value; } }
        private IIndexSearch _indexSearch{ get => TestContext.Properties["ii"] as IIndexSearch; set { TestContext.Properties["ii"] = value; } }

        private PageReference _dataPage { get => TestContext.Properties["dp"] as PageReference; set { TestContext.Properties["dp"] = value; } }
        [TestInitialize]
        public void Init()
        {
            var pm = new FactoryStub().CreateManager("",new IndexFileConfiguration(),false);
            _page1 =pm.GetRecordAccessor<IndexPageRecord>(pm.CreatePage(1));
            var root1 = _page1.AddRecord(new IndexPageRecord { Start = 0, End = 0, MaxUnderlyingDepth = 0 });
            var pm2 = new FactoryStub().CreateManager("", new IndexFileConfiguration(), false);
            _page2 = pm2.GetRecordAccessor<IndexPageRecord>(pm2.CreatePage(1));
            _page2.AddRecord(new IndexPageRecord { Start = 0, End = 0, MaxUnderlyingDepth = 0 });
            _indexInteraction = new IndexInteractor(_page1, _page2, root1.Reference);

            var pm3 = new FactoryStub().CreateManager("", new IndexFileConfiguration(), false);
            _dataPage = pm3.CreatePage(1);
        }

        private DataPageRef testRef(long start,long end)
        {            
            return new DataPageRef { Start = start, End = end, DataReference = _dataPage };
        }

        [TestMethod]
        public void GetRootWhenNoRoot()
        {
            Assert.IsNull(_indexInteraction.GetRoot());
        }


        [TestMethod]
        public async Task InsertDatatWhenNoRoot()
        {
            await _indexInteraction.PrepareIndexChange();
            var dataRef = testRef(1, 3);
            _indexInteraction.CreateDataBlock(dataRef);
            await _indexInteraction.FinalizeIndexChange();
            checkBothPages(page =>
            {
                var root = page.IterateRecords().First();
                Assert.AreEqual(1, root.Data.Start);
                Assert.AreEqual(3, root.Data.End);
                Assert.AreEqual(0, root.Data.MaxUnderlyingDepth);
                Assert.AreEqual(dataRef.DataReference, root.Data.Data);
                Assert.AreEqual(null, root.Data.ChildrenTwo);
            });
            
        }

        [TestMethod]
        public async Task GetRootWhenThereIsRoot()
        {
            await _indexInteraction.PrepareIndexChange();
            var dataRef = testRef(1, 3);
            _indexInteraction.CreateDataBlock(dataRef);
            await _indexInteraction.FinalizeIndexChange();
            var root = _indexInteraction.GetRoot();
            Assert.IsNotNull(root);
            Assert.AreEqual(1, root.Value.Start);
            Assert.AreEqual(3, root.Value.End);
            Assert.AreEqual(0, root.Value.MaxUnderlyingDepth);
            Assert.IsTrue(root.Value.StoresData);
            

        }

        [TestMethod]
        public async Task ResizeIndex()
        {
            await _indexInteraction.PrepareIndexChange();
            var dataRef = testRef(1, 3);
            _indexInteraction.CreateDataBlock(dataRef);
            await _indexInteraction.FinalizeIndexChange();
            await _indexInteraction.PrepareIndexChange();
            var root = _indexInteraction.GetRoot().Value;
            root = _indexInteraction.ResizeIndex(root, 2, 10);
            await _indexInteraction.FinalizeIndexChange();
            
            Assert.AreEqual(2, root.Start);
            Assert.AreEqual(10, root.End);
            Assert.AreEqual(0, root.MaxUnderlyingDepth);
            Assert.IsTrue(root.StoresData);

            checkBothPages(page =>
            {
                var rt = page.IterateRecords().First();
                Assert.AreEqual(2, rt.Data.Start);
                Assert.AreEqual(10, rt.Data.End);
                Assert.AreEqual(0, rt.Data.MaxUnderlyingDepth);
            });

        }

        [TestMethod]
        public async Task GetDataRefFromRecord()
        {
            await _indexInteraction.PrepareIndexChange();
            var dataRef = testRef(1, 3);
            _indexInteraction.CreateDataBlock(dataRef);
            await _indexInteraction.FinalizeIndexChange();
            var root = _indexInteraction.GetRoot();
            var insertedRef = _indexSearch.GetDataRef(root.Value);
            Assert.IsNotNull(root);
            Assert.AreEqual(insertedRef.Start, dataRef.Start);
            Assert.AreEqual(insertedRef.End, dataRef.End);
            Assert.AreEqual(insertedRef.DataReference, dataRef.DataReference);
        }

        [TestMethod]
        public async Task InsertDataWhenRootIsData()
        {
            await _indexInteraction.PrepareIndexChange();
            var dataRef = testRef(1, 3);
            _indexInteraction.CreateDataBlock(dataRef);
            await _indexInteraction.FinalizeIndexChange();

            await _indexInteraction.PrepareIndexChange();
            var dataRef2 = testRef(3, 4);
            var dataBlock = _indexInteraction.CreateDataBlock(_indexInteraction.GetRoot().Value, dataRef2);
            await _indexInteraction.FinalizeIndexChange();
            Assert.AreEqual(3, dataBlock.Start);
            Assert.AreEqual(4, dataBlock.End);
            Assert.AreEqual(0, dataBlock.MaxUnderlyingDepth);
            Assert.IsTrue(dataBlock.StoresData);

            checkBothPages(page => {
                var root = page.IterateRecords().First();
                Assert.AreEqual(1, root.Data.Start);
                Assert.AreEqual(3, root.Data.End);
                Assert.AreEqual(1, root.Data.MaxUnderlyingDepth);
                Assert.IsNotNull(root.Data.ChildrenOne);
                Assert.IsNotNull(root.Data.ChildrenTwo);

                var childOne = root.Data.ChildrenOne;
                var data1 = page.GetRecord(childOne);
                Assert.AreEqual(dataRef.DataReference, data1.Data.Data);
                Assert.IsNull(data1.Data.ChildrenOne);
                Assert.IsNull(data1.Data.ChildrenTwo);
                Assert.AreEqual(1, data1.Data.Start);
                Assert.AreEqual(3, data1.Data.End);
                Assert.AreEqual(0, data1.Data.MaxUnderlyingDepth);

                var childTwo = root.Data.ChildrenTwo;
                var data2 = page.GetRecord(childTwo);
                Assert.AreEqual(dataRef2.DataReference, data2.Data.Data);
                Assert.IsNull(data2.Data.ChildrenOne);
                Assert.IsNull(data2.Data.ChildrenTwo);
                Assert.AreEqual(3, data2.Data.Start);
                Assert.AreEqual(4, data2.Data.End);
                Assert.AreEqual(0, data2.Data.MaxUnderlyingDepth);
            });
         
        }

        [TestMethod]
        public async Task GetChildrenForRootWithTwoLeaves()
        {
            await _indexInteraction.PrepareIndexChange();
            var dataRef = testRef(1, 3);
            _indexInteraction.CreateDataBlock(dataRef);
            await _indexInteraction.FinalizeIndexChange();

            await _indexInteraction.PrepareIndexChange();
            var dataRef2 = testRef(3, 4);
            _indexInteraction.CreateDataBlock(_indexInteraction.GetRoot().Value, dataRef2);
            await _indexInteraction.FinalizeIndexChange();
            var children = _indexInteraction.GetChildren(_indexInteraction.GetRoot().Value);


            Assert.AreEqual(1, children[0].Start);
            Assert.AreEqual(3, children[0].End);
            Assert.AreEqual(0, children[0].MaxUnderlyingDepth);
            Assert.IsTrue(children[0].StoresData);

            Assert.AreEqual(3, children[1].Start);
            Assert.AreEqual(4, children[1].End);
            Assert.AreEqual(0, children[1].MaxUnderlyingDepth);
            Assert.IsTrue(children[1].StoresData);

        }


        [TestMethod]
        public async Task MoveIndex()
        {
            await _indexInteraction.PrepareIndexChange();
            var dataRef = testRef(1, 3);
            var dataRef2 = testRef(3, 4);
            var dataRef3 = testRef(4, 5);
            _indexInteraction.CreateDataBlock(dataRef);
            var rootWhereMoveTo =_indexInteraction.CreateDataBlock(_indexInteraction.GetRoot().Value, dataRef2);
            var child = _indexInteraction.GetChildren(_indexInteraction.GetRoot().Value)[0];
            var childToMove = _indexInteraction.CreateDataBlock(child, dataRef3);
            await _indexInteraction.FinalizeIndexChange();

            await _indexInteraction.PrepareIndexChange();
            rootWhereMoveTo = _indexInteraction.MoveIndex(rootWhereMoveTo, childToMove);            
            await _indexInteraction.FinalizeIndexChange();

            checkBothPages(page =>
            {
                var root = page.IterateRecords().First();
                Assert.AreEqual(1, root.Data.Start);
                Assert.AreEqual(3, root.Data.End);
                Assert.AreEqual(1, root.Data.MaxUnderlyingDepth);
                var data1 = page.GetRecord(root.Data.ChildrenOne);
                Assert.AreEqual(1, data1.Data.Start);
                Assert.AreEqual(3, data1.Data.End);
                Assert.AreEqual(dataRef.DataReference, data1.Data.Data);
                Assert.IsNull(data1.Data.ChildrenOne);
                Assert.IsNull(data1.Data.ChildrenTwo);

                var index = page.GetRecord(root.Data.ChildrenTwo);
                Assert.AreEqual(3, index.Data.Start);
                Assert.AreEqual(4, index.Data.End);
                Assert.AreEqual(1, index.Data.MaxUnderlyingDepth);
                var data2 = page.GetRecord(index.Data.ChildrenOne);                
                Assert.AreEqual(3, data2.Data.Start);
                Assert.AreEqual(4, data2.Data.End);
                Assert.AreEqual(0, data2.Data.MaxUnderlyingDepth);
                Assert.AreEqual(dataRef2.DataReference, data2.Data.Data);
                Assert.IsNull(data2.Data.ChildrenOne);
                Assert.IsNull(data2.Data.ChildrenTwo);

                var data3 = page.GetRecord(index.Data.ChildrenTwo);
                Assert.AreEqual(4, data3.Data.Start);
                Assert.AreEqual(5, data3.Data.End);
                Assert.AreEqual(0, data3.Data.MaxUnderlyingDepth);
                Assert.AreEqual(dataRef3.DataReference, data3.Data.Data);
                Assert.IsNull(data3.Data.ChildrenOne);
                Assert.IsNull(data3.Data.ChildrenTwo);
            });

            Assert.AreEqual(3, rootWhereMoveTo.Start);
            Assert.AreEqual(4, rootWhereMoveTo.End);
            Assert.AreEqual(1, rootWhereMoveTo.MaxUnderlyingDepth);
            Assert.IsFalse(rootWhereMoveTo.StoresData);           

        }

        private void checkBothPages(Action<IPage<IndexPageRecord>> check)
        {
            check(_page1);
            check(_page2);
        }
    }
}
