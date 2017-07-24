using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pager;
using Rhino.Mocks;

namespace Test.Pager
{
    [TestClass]
    public class PageAccessorTest
    {
        public TestContext TestContext { get; set; }
        private IPageAccessor Create()
        {
            var m = MemoryMappedFile.CreateFromFile(TestContext.TestName,System.IO.FileMode.OpenOrCreate,TestContext.TestName,8192);
            var acc = m.CreateViewAccessor(0, 8192);
            var p = new MockRepository().StrictMock<IExtentAccessorFactory>();
            p.Expect(k => k.ReturnAccessor(null)).IgnoreArguments().Repeat.Any().Do(new Action<MemoryMappedViewAccessor>(k2 => k2.Dispose()));
            p.Replay();
            var t = new PageAccessor(4096, 4096,1, acc, p);
            TestContext.Properties.Add("Map", m);
            return t;
        }
        private MemoryMappedFile map => TestContext.Properties["Map"] as MemoryMappedFile;

        [TestMethod]
        public void GetByteArray()
        {
            var arr1 = Enumerable.Range(0, 4096).Select(k => (byte)1).ToArray();
            var arr2 = Enumerable.Range(0, 4096).Select(k => (byte)2).ToArray();
            var accessor = Create();
            using (var s = map.CreateViewStream())
                s.Write(arr1.Concat(arr2).ToArray(), 0, 8192);

            
            var page = accessor.GetByteArray(0, 4096);
            CollectionAssert.AreEquivalent(arr2, page);

            accessor.Dispose();
            map.Dispose();
            File.Delete(TestContext.TestName);
        }

        [TestMethod]
        public void SetByteArray()
        {
            var arr1 = Enumerable.Range(0, 4096).Select(k => (byte)0).ToArray();
            var arr2 = Enumerable.Range(0, 4096).Select(k => (byte)2).ToArray();
            
            var accessor = Create();
            accessor.SetByteArray(arr2,0, 4096);
            byte[] page = new byte[8192];
            using (var s = map.CreateViewStream())
                s.Read(page, 0, 8192);

            CollectionAssert.AreEquivalent(arr1.Concat(arr2).ToArray(), page);

            map.Dispose();
            accessor.Dispose();
            File.Delete(TestContext.TestName);
        }
    }
}
