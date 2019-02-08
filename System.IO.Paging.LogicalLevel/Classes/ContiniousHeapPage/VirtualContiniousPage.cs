using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Paging.Diagnostics;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.Linq;

namespace System.IO.Paging.LogicalLevel.Classes.ContiniousHeapPage
{
    internal sealed class VirtualContiniousPage<TRecord>:IPage<TRecord>
        where TRecord:struct
         
    {
        private readonly IPageManager _physicalPageManager;
        private readonly byte _headerPageTypeNum;
        private ConcurrentBag<PageReference> _headersPage;
        private TypedRecord<HeapHeader> _theBestCandidate;
        public VirtualContiniousPage(IPageManager physicalPageManager,  byte pageType, byte headerPageTypeNum)
        {
            _physicalPageManager = physicalPageManager;
            RegisteredPageType = pageType;
            _headerPageTypeNum = headerPageTypeNum;
            Reference = new VirtualPageReference(0,pageType);
            PageFullness = 0;

            _headersPage = new ConcurrentBag<PageReference>(_physicalPageManager.IteratePages(headerPageTypeNum));
             if (!_headersPage.Any())
                _headersPage.Add(_physicalPageManager.CreatePage(headerPageTypeNum));


            FindOrCreateNewCandidate();                    
        }

        private void FindOrCreateNewCandidate()
        {
            var newCanAct = Tracing.Tracer.StartActivity(new Activity($"Looking for a new candidate for record placement."),null);
          
            var theBestCandidate =
                _headersPage.Select(_physicalPageManager.GetRecordAccessor<HeapHeader>)
                    .SelectMany(k => k.IterateRecords())
                    .Where(k => k != null)
                    .FirstOrDefault(k => k.Data.Fullness < .95);
            
            if (theBestCandidate == null)
            {
                Tracing.Tracer.StopActivity(newCanAct,"nothing found");
                var creatingAct = Tracing.Tracer.StartActivity(new Activity("Creating new candidate"), null);
                var newPage = _physicalPageManager.CreatePage(RegisteredPageType);
                var candidate = new HeapHeader
                {
                    Fullness = 0,
                    LogicalPageNum = (uint)newPage.PageNum
                };
                theBestCandidate = _headersPage.Select(_physicalPageManager.GetRecordAccessor<HeapHeader>).Select(k => k.AddRecord(candidate)).FirstOrDefault(k => k != null);
                if (theBestCandidate == null)
                {
                    var newHeaderPage = _physicalPageManager.CreatePage(_headerPageTypeNum);
                    _headersPage.Add(newHeaderPage);
                    using (var acc = _physicalPageManager.GetRecordAccessor<HeapHeader>(newHeaderPage))
                        theBestCandidate = acc.AddRecord(candidate);

                }
                Tracing.Tracer.StopActivity(creatingAct,null);
            }
            else
            {
                Tracing.Tracer.StopActivity(newCanAct,"found");
            }
            _theBestCandidate = theBestCandidate;
        }

        public void Dispose()
        {
          
        }

        public byte RegisteredPageType { get; }
        public PageReference Reference { get; }
        public double PageFullness { get; }

        private object _lockCreation = new object();

        public TypedRecord<TRecord> AddRecord(TRecord type)
        {
            TypedRecord<TRecord> recordAdded = null;
            var act = Tracing.Tracer.StartActivity(new Activity($"Adding record to virtual continuous page"),type);
            var searchCandAct = Tracing.Tracer.StartActivity(new Activity("Candidate search"), type);
            while (recordAdded == null)
            {
                var currentCandidate = _theBestCandidate;
                var pageRef = new PageReference((int)currentCandidate.Data.LogicalPageNum);
                Tracing.Tracer.StopActivity(searchCandAct,null);
            
                using (var page =
                    _physicalPageManager.GetRecordAccessor<TRecord>(pageRef))
                {
                    recordAdded = page.AddRecord(type);
                    if (recordAdded != null)
                    {
                        var recordAdd = Tracing.Tracer.StartActivity(new Activity("Updating virtual header"), type);
                        currentCandidate.Data.Fullness = _physicalPageManager
                            .GetPageInfo(new PageReference((int)currentCandidate.Data.LogicalPageNum))
                            .PageFullness;
                        using (var headersPage = _physicalPageManager.GetRecordAccessor<HeapHeader>(currentCandidate.Reference.Page))
                            headersPage.StoreRecord(currentCandidate);
                        Tracing.Tracer.StopActivity(recordAdd,null);
                    }
                }
                if (recordAdded == null)
                {
                    Tracing.Tracer.Write($"Chosen physical page is full!! Will find another.",null);
                    currentCandidate.Data.Fullness = 1;
                    using (var headersPage = _physicalPageManager.GetRecordAccessor<HeapHeader>(currentCandidate.Reference.Page))
                        headersPage.StoreRecord(currentCandidate);
                    try
                    {
                        FindOrCreateNewCandidate();                       
                    }
                    catch (InvalidOperationException)
                    {
                        Trace.Unindent();
                        return null;
                    }
                }
            }

            Trace.Unindent();
            return recordAdded;
        }

        public void FreeRecord(TypedRecord<TRecord> record)
        {
            var act = Tracing.Tracer.StartActivity(new Activity($"Freeing record"),(record.Reference,record.Data));            
            using (var pageInfo = _physicalPageManager.GetPageInfo(record.Reference.Page))
            using (var page = _physicalPageManager.GetRecordAccessor<TRecord>(record.Reference.Page))
            {

                if (page == null)
                    return;
                if (pageInfo.RegisteredPageType != RegisteredPageType)
                    throw new InvalidOperationException($"The record does not belong to this page");
                page.FreeRecord(record);
                
                foreach (var headerPageRef in _headersPage)
                {

                    using (var headersPage = _physicalPageManager.GetRecordAccessor<HeapHeader>(headerPageRef))
                    {
                        var curHeader = headersPage
                            .IterateRecords()
                            .FirstOrDefault(k => k.Data.LogicalPageNum == pageInfo.Reference.PageNum);
                        if (curHeader != null)
                        {
                            curHeader.Data.Fullness = pageInfo.PageFullness;
                            headersPage.StoreRecord(curHeader);
                            break;
                        }
                    }
                    Tracing.Tracer.StopActivity(act,null);
                }
            }
        }

        public TypedRecord<TRecord> GetRecord(PageRecordReference reference)
        {
            var act = Tracing.Tracer.StartActivity(new Activity($"Getting record from virtual continuous page"),reference);
          
            using (var pageInfo = _physicalPageManager.GetPageInfo(reference.Page))
            using (var page = _physicalPageManager.GetRecordAccessor<TRecord>(reference.Page))
            {
                if (page == null)
                    return null;
                if (pageInfo.RegisteredPageType != RegisteredPageType)
                {                    
                    throw new InvalidOperationException($"The record does not belong to this page");
                }
                var rec = page.GetRecord(reference);
                Tracing.Tracer.StopActivity(act,rec.Data);
                return rec;
            }
        }

        public void StoreRecord(TypedRecord<TRecord> record)
        {
            var act = Tracing.Tracer.StartActivity(new Activity($"Storing record  to virtual continuous page"),(record.Reference,record.Data));
           
            using (var pageInfo = _physicalPageManager.GetPageInfo(record.Reference.Page))
            using (var page = _physicalPageManager.GetRecordAccessor<TRecord>(record.Reference.Page))
            {
                if (page == null)
                    return;
                if (pageInfo.RegisteredPageType != RegisteredPageType)
                    throw new InvalidOperationException($"The record does not belong to this page");
                page?.StoreRecord(record);
                Tracing.Tracer.StopActivity(act,null);
            }
        }

        public IEnumerable<TypedRecord<TRecord>> GetRecordRange(PageRecordReference start, PageRecordReference end)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TypedRecord<TRecord>> IterateRecords()
        {
            Tracing.Tracer.Write($"Starting iterating records in virtual continuous page",null);
            foreach (var headerPageRef in _headersPage)
            using (var headersPage = _physicalPageManager.GetRecordAccessor<HeapHeader>(headerPageRef))
            {
                foreach (var header in headersPage.IterateRecords())
                {
                    using (var page =
                        _physicalPageManager.GetRecordAccessor<TRecord>(
                            new PageReference((int) header.Data.LogicalPageNum)))
                    {
                        if (page != null)
                            foreach (var record in page.IterateRecords())
                            {
                                yield return record;
                            }
                    }
                }
            }
        }

        public IBinarySearcher<TRecord> BinarySearch()
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            
        }
    }
}
