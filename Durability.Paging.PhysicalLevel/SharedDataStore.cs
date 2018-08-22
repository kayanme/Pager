using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.References;

namespace Durability.Paging.PhysicalLevel
{
    internal class SharedDataStore
    {

        private readonly ConcurrentDictionary<PageRecordReference, TestRecord> _store = new ConcurrentDictionary<PageRecordReference, TestRecord>();
        private readonly Random _rnd = new Random();

        private readonly HashSet<PageRecordReference> _inUse = new HashSet<PageRecordReference>();

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
            lock (_inUse)
                _inUse.Remove(reference);
        }

        public void Delete(PageRecordReference reference,TestRecord old)
        {            
            if (!_store.TryRemove(reference, out var rec) || rec.Data != old.Data)
                Debugger.Break();
            lock(_inUse)
            _inUse.Remove(reference);
        }

        public Tuple<PageRecordReference,TestRecord> SelectRandom()
        {
            if (!_store.Any())
                return null;
            try
            {
                PageRecordReference key = null;
                bool inUse = false ;
                int count = 1000;
                do
                {
                    var keyNum = _rnd.Next(_store.Count);
                    try
                    {
                        key = _store.Keys.ToArray()[keyNum];
                        lock (_inUse)
                            inUse = _inUse.Add(key);
                        if (count <0 && !inUse)
                            lock (_inUse)
                                _inUse.Remove(key);
                    }
                    catch (IndexOutOfRangeException) { }
                    count--;
                }
                while (!inUse);                        
                return Tuple.Create(key,_store[key]);
            }
            catch
            {
                return null;
            }
        }

        public void Check(Tuple<PageRecordReference,TestRecord> data)
        {            
            if (!_store.TryGetValue(data.Item1,out var rec) || rec.Data != data.Item2.Data)
                Debugger.Break();
        }
    }
}
