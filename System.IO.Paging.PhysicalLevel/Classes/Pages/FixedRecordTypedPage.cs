using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.IO.Paging.Diagnostics;
using System.Linq;

namespace System.IO.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class FixedRecordTypedPage<TRecordType> : TypedPageBase,IPage<TRecordType> where TRecordType : struct
    {
        public IPageHeaders Headers { get; }
        private readonly IRecordAcquirer<TRecordType> _recordGetter;             
        private readonly FixedRecordTypePageConfiguration<TRecordType> _config;
        private KeyPersistanseType _keyType;
        internal FixedRecordTypedPage(IPageHeaders headers,
            IRecordAcquirer<TRecordType> recordGetter, PageReference reference, 
            FixedRecordTypePageConfiguration<TRecordType> config,
            Action actionToClean):base(reference, actionToClean)
        {
            Headers = headers;
            _recordGetter = recordGetter;                
            _config = config;
            _keyType = config.WithLogicalSort ? KeyPersistanseType.Logical : KeyPersistanseType.Physical;
        }


        public  TypedRecord<TRecordType> GetRecord(PageRecordReference reference)
        {
            CheckReferenceToPageAffinity(reference);
            if (reference is NullPageRecordReference)
                return null;
            var act = new Activity("Getting record from fixed size record page");
            Tracing.Tracer.StartActivity(act, reference);
            var sw = Stopwatch.StartNew();
            Debug.Assert(reference is PhysicalPositionPersistentPageRecordReference,
                "reference is PhysicalPositionPersistentPageRecordReference");
            if (!Headers.IsRecordFree(reference.PersistentRecordNum))
            {
                var offset = reference.PersistentRecordNum;
                var size = Headers.RecordSize(reference.PersistentRecordNum);
                TypedRecord<TRecordType> r2 = new TypedRecord<TRecordType>{Reference = reference};
                r2.Data=_recordGetter.GetRecord(offset, size);
                Tracing.Tracer.StopActivity(act,r2.Data);
                return r2;
            }
            Tracing.Tracer.StopActivity(act, null);          
            return null;

        }

        public TypedRecord<TRecordType> AddRecord(TRecordType type)
        {
            var act = new Activity("Adding record to fixed size record page");

            act = Tracing.Tracer.StartActivity(act, type);
            var headerAct = new Activity("Header allocation in fixed size record page");
            headerAct.SetParentId(act.Id);
            headerAct = Tracing.Tracer.StartActivity(headerAct,type);
            var physicalRecordNum = Headers.TakeNewRecord(0, (ushort) _config.RecordMap.GetSize);
            Tracing.Tracer.StopActivity(headerAct, physicalRecordNum);                                  
            if (physicalRecordNum == -1)
                return null;
            SetRecord(physicalRecordNum, type);
            
            var d = new TypedRecord<TRecordType>
            {
                Data = type,
                Reference = new PhysicalPositionPersistentPageRecordReference(Reference, (ushort) physicalRecordNum)
            };
            Tracing.Tracer.StopActivity(act,d.Reference );            
            return d;

        }

        private  void SetRecord(int logicalRecordNum, TRecordType record)
        {
            var act = Tracing.Tracer.StartActivity(new Activity("Stored record by log. shift to fixed size record page"),record);
            var recordStart = Headers.RecordShift((ushort)logicalRecordNum);
            var recordSize = Headers.RecordSize((ushort)logicalRecordNum);
            _recordGetter.SetRecord(recordStart, recordSize,record);           
            Tracing.Tracer.StopActivity(act,null);
        }

        private  void SetRecord(ushort physicalRecordNum, TRecordType record)
        {
            var act = Tracing.Tracer.StartActivity(new Activity("Stored record by log. shift to fixed size record page"), record);

            var recordSize = (ushort) _config.RecordMap.GetSize;
            _recordGetter.SetRecord(physicalRecordNum, recordSize, record);
            Tracing.Tracer.StopActivity(act, null);
        }

        public void StoreRecord(TypedRecord<TRecordType> record)
        {
            if (record.Reference.Page != Reference)
                throw new ArgumentException();          
            SetRecord(record.Reference.PersistentRecordNum, record.Data);

        }

        private IEnumerable<T> InRange<T>(IEnumerable<T> source,Func<T,bool> start,Func<T,bool> end)
        {
            bool started = false;
            foreach(var s in source)
            {
                if (!started && start(s))
                {
                    started = true;
                }
                if (started)
                    yield return s;
                if (end(s))
                    yield break;
            }
        }

        public IEnumerable<TypedRecord<TRecordType>> GetRecordRange(PageRecordReference start, PageRecordReference end)
        {
            CheckReferenceToPageAffinity(start);
            CheckReferenceToPageAffinity(end);
            Tracing.Tracer.Write($"Search for records in range:",(start,end));
            var nonFreeHeaders = InRange(Headers.NonFreeRecords(),k=>k ==start.PersistentRecordNum,k=>k==end.PersistentRecordNum).ToArray();
            foreach (var nonFreeRecord in nonFreeHeaders)
            {
                var shift = Headers.RecordShift(nonFreeRecord);
                var size = Headers.RecordSize(nonFreeRecord);
                var t = _recordGetter.GetRecord(shift, size);
                var reference = PageRecordReference.CreateReference(Reference, nonFreeRecord, _keyType);
                yield return new TypedRecord<TRecordType> { Data = t, Reference = reference };
            }
        }

        public void FreeRecord(TypedRecord<TRecordType> record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            if (record.Reference is NullPageRecordReference)
                throw new ArgumentException("Trying to delete deleted record");
            var act = Tracing.Tracer.StartActivity(new Activity("Freeing record"), (record.Reference, record.Data));

            var sw = Stopwatch.StartNew();
            Headers.FreeRecord((ushort)record.Reference.PersistentRecordNum);
            record.Reference = new NullPageRecordReference(Reference);
            Tracing.Tracer.StopActivity(act, null);
        }
    

        public IEnumerable<TypedRecord<TRecordType>> IterateRecords()
        {          
            var s = (ushort) _config.RecordMap.GetSize;
            Tracing.Tracer.Write($"Starting iterate records", Reference);          
            var sw = Stopwatch.StartNew();
            foreach (var nonFreeRecord in Headers.NonFreeRecords())
            {
                var shift = Headers.RecordShift(nonFreeRecord);
                var t = _recordGetter.GetRecord(shift, s);
                var reference = PageRecordReference.CreateReference(Reference,nonFreeRecord,_keyType);                    
                yield return new TypedRecord<TRecordType>{Data = t,Reference = reference};
                sw.Restart();
            }           

        }

      

        public void Flush()
        {
            var act = Tracing.Tracer.StartActivity(new Activity($"Starting flush"),Reference);
          
            _recordGetter.Flush();
            Tracing.Tracer.StopActivity(act,null);
        }


        ~FixedRecordTypedPage()
        {
            Dispose(true);
        
        }

       
    }
}
