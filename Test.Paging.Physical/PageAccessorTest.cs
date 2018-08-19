using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using FakeItEasy;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;

namespace Test.Paging.PhysicalLevel
{
    [TestClass]
    public class PageAccessorTest
    {
        public TestContext TestContext { get; set; }
        private IPageAccessor Create()
        {
            var m = MemoryMappedFile.CreateFromFile(TestContext.TestName,FileMode.OpenOrCreate,TestContext.TestName,8192);
            var acc = m.CreateViewAccessor(0, 8192);
            var p = A.Fake<IExtentAccessorFactory>();
            A.CallTo(()=> p.ReturnAccessor(null)).WithAnyArguments().Invokes((k => (k.Arguments[0] as MemoryMappedViewAccessor).Dispose()));
          
            var t = new PageAccessor(4096, 4096,1, acc, p);
            TestContext.Properties.Add("Map", m);
            return t;
        }
        private MemoryMappedFile Map => TestContext.Properties["Map"] as MemoryMappedFile;

        [TestMethod]
        public void GetByteArray()
        {
            var arr1 = Enumerable.Range(0, 4096).Select(k => (byte)1).ToArray();
            var arr2 = Enumerable.Range(0, 4096).Select(k => (byte)2).ToArray();
            var accessor = Create();
            using (var s = Map.CreateViewStream())
                s.Write(arr1.Concat(arr2).ToArray(), 0, 8192);

            
            var page = accessor.GetByteArray(0, 4096);
            CollectionAssert.AreEquivalent(arr2, page);

            accessor.Dispose();
            Map.Dispose();
            System.IO.File.Delete(TestContext.TestName);
        }

        [TestMethod]
        public void SetByteArray()
        {
            var arr1 = Enumerable.Range(0, 4096).Select(k => (byte)0).ToArray();
            var arr2 = Enumerable.Range(0, 4096).Select(k => (byte)2).ToArray();
            
            var accessor = Create();
            accessor.SetByteArray(arr2,0, 4096);
            byte[] page = new byte[8192];
            using (var s = Map.CreateViewStream())
                s.Read(page, 0, 8192);

            CollectionAssert.AreEquivalent(arr1.Concat(arr2).ToArray(), page);

            Map.Dispose();
            accessor.Dispose();
            System.IO.File.Delete(TestContext.TestName);
        }
    }
}
