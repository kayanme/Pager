using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Paging.LogicalLevel;
using System.IO.Paging.LogicalLevel.Configuration;
using System.IO.Paging.LogicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration.Builder;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Durability.Paging.LogicalLevel.Core
{
    public struct Data:IFixedSizeRecordDefinition<Data>
    {
        public byte[] T;
        
        public int Size => 8;

        public void FillBytes(ref Data record, byte[] targetArray)
        {
           for(int i = 0; i < 8; i++)
            {
                targetArray[i] = record.T[i];
            }
        }

        public void FillFromBytes(byte[] sourceArray, ref Data record)
        {
            record.T = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                record.T[i] = sourceArray[i];
            }
        }
    }
    public class C : LogicalPageManagerConfiguration
    {
        public C():base(PageSize.Kb4)
        {
            DefinePageType(1).AsPageWithRecordType<Data>()
                  .UsingRecordDefinition(new Data())
                  .AsVirtualHeapPage(2);
        }
    }

    public static class Program
    {
        private static ConcurrentBag<PageRecordReference> _usedRecords = new ConcurrentBag<PageRecordReference>();

        public async static Task Main()
        {
           
            var pmf = new LogicalPageManagerFactory();
            using (var pm = pmf.CreateManagerWithAutoFileCreation("test", new C()))
            {
                await Task.WhenAll(
                    AddTask(pm),
                    AddTask(pm),
                    StoreTask(pm),
                    StoreTask(pm),
                    DeleteTask(pm),
                    DeleteTask(pm)
                    );
            }
         
            File.Delete("test");
        }

        public async static Task AddTask(IPageManager pageManager)
        {
            await Task.Yield();
            for (int i = 0; i < 1E4; i++)
            {
                using(var a = pageManager.GetRecordAccessor<Data>(pageManager.CreatePage(1)))
                {
                    var t = a.AddRecord(new Data { T = new byte[8]});
                    _usedRecords.Add(t.Reference);
                }
            }
        }

        public async static Task StoreTask(IPageManager pageManager)
        {            
            await Task.Yield();
            for (int i = 0; i < 1E4; i++)
            {
                if (_usedRecords.TryTake(out var r))
                    using (var a = pageManager.GetRecordAccessor<Data>(pageManager.CreatePage(1)))
                    {
                        var t = a.GetRecord(r);
                        a.StoreRecord(t);
                        _usedRecords.Add(r);
                    }
            }
        }

        public async static Task DeleteTask(IPageManager pageManager)
        {
            await Task.Yield();
            for (int i = 0; i < 1E4; i++)
            {
                if (_usedRecords.TryTake(out var r))
                    using (var a = pageManager.GetRecordAccessor<Data>(pageManager.CreatePage(1)))
                    {
                        var t = a.GetRecord(r);
                        a.FreeRecord(t);                        
                    }
            }
        }
    }

}
