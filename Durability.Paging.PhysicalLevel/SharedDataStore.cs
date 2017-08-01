using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager;

namespace Durability.Paging.PhysicalLevel
{
    internal class SharedDataStore
    {

        private ConcurrentDictionary<PageRecordReference, TestRecord> _store = new ConcurrentDictionary<PageRecordReference, TestRecord>();
        private Random _rnd = new Random();

        private HashSet<PageRecordReference> _inUse = new HashSet<PageRecordReference>();

        public void Add(PageRecordReference reference,TestRecord data)
        {
            if (!_store.TryAdd(reference, data))
                Debugger.Break();
        }

        public void Update(PageRecordReference reference, TestRecord old, TestRecord data)
        {
            _store.AddOrUpdate(reference, data, (k, o) =>
             {
                 if (o.Data != old.Data)
                     Debugger.Break();
                 return data;

             });
            _inUse.Remove(reference);
        }

        public void Delete(PageRecordReference reference,TestRecord old)
        {
            TestRecord rec;
            if (!_store.TryRemove(reference, out rec) || rec.Data != old.Data)
                Debugger.Break();
            _inUse.Remove(reference);
        }

        public TestRecord SelectRandom()
        {
            if (!_store.Any())
                return null;
            try
            {
                PageRecordReference key;
                do
                {
                    var keyNum = _rnd.Next(_store.Count);
                    key = _store.Keys.ToArray()[keyNum];
                }
                while (!_inUse.Add(key));                        
                return _store[key];
            }
            catch
            {
                return null;
            }
        }

        public void Check(TestRecord data)
        {
            TestRecord rec;
            if (!_store.TryGetValue(data.Reference,out rec) || rec.Data != data.Data)
                Debugger.Break();
        }
    }
}
