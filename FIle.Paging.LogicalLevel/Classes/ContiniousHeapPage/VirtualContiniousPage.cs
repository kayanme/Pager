using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;
using File.Paging.PhysicalLevel.Contracts;

namespace FIle.Paging.LogicalLevel.Classes.ContiniousHeapPage
{
     internal sealed class VirtualContiniousPage<TRecord>:IPage<TRecord>
        where TRecord:struct
         
    {
        private readonly IPageManager _physicalPageManager;
        private PageReference _headersPage;
        private TypedRecord<HeapHeader> _theBestCandidate;
        public VirtualContiniousPage(IPageManager physicalPageManager,  byte pageType, byte headerPageTypeNum)
        {
            _physicalPageManager = physicalPageManager;
            RegisteredPageType = pageType;
            Reference = new VirtualPageReference(0,pageType);
            PageFullness = 0;

            _headersPage = (_physicalPageManager.IteratePages(headerPageTypeNum).FirstOrDefault()??
                                _physicalPageManager.CreatePage(headerPageTypeNum));
            
            FindOrCreateNewCandidate();
        }

        private void FindOrCreateNewCandidate()
        {
            using (var headersPage = _physicalPageManager.GetRecordAccessor<HeapHeader>(_headersPage))
            {
                var theBestCandidate =
                    headersPage
                        .IterateRecords()                       
                        .Where(k => k != null)
                        .FirstOrDefault(k => k.Data.Fullness < .95);

                if (theBestCandidate == null)
                {
                    var newPage = _physicalPageManager.CreatePage(RegisteredPageType);
                    var candidate = new HeapHeader
                    {
                        Fullness = 0,
                        LogicalPageNum = (uint) newPage.PageNum
                    };
                    theBestCandidate = headersPage.AddRecord(candidate);
                    if (theBestCandidate == null)
                    {
                        _physicalPageManager.DeletePage(newPage, false);
                        throw new InvalidOperationException("No more records allowed");
                    }
                   
                
                }
                _theBestCandidate = theBestCandidate;
            }
        }

        public void Dispose()
        {
          
        }

        public byte RegisteredPageType { get; }
        public PageReference Reference { get; }
        public double PageFullness { get; }
      
        public TypedRecord<TRecord> AddRecord(TRecord type)
        {
            TypedRecord<TRecord> recordAdded = null;
            using (var headersPage = _physicalPageManager.GetRecordAccessor<HeapHeader>(_headersPage))
            {
                while (recordAdded==null)
                {
                    var currentCandidate = _theBestCandidate;
                    using (var page =
                        _physicalPageManager.GetRecordAccessor<TRecord>(
                            new PageReference((int) currentCandidate.Data.LogicalPageNum)))
                    {
                        recordAdded = page.AddRecord(type);
                        if (recordAdded != null)
                        {
                            currentCandidate.Data.Fullness = _physicalPageManager
                                .GetPageInfo(new PageReference((int) currentCandidate.Data.LogicalPageNum))
                                .PageFullness;
                            headersPage.StoreRecord(currentCandidate);
                        }
                    }
                    if (recordAdded==null)
                    {
                        currentCandidate.Data.Fullness = 1;
                        headersPage.StoreRecord(currentCandidate);
                        try
                        {
                            FindOrCreateNewCandidate();

                        }
                        catch (InvalidOperationException)
                        {
                            return null;
                        }
                    }
                }
            }
         
            return recordAdded;
        }

        public void FreeRecord(TypedRecord<TRecord> record)
        {
            using (var pageInfo = _physicalPageManager.GetPageInfo(record.Reference.Page))
            using (var page = _physicalPageManager.GetRecordAccessor<TRecord>(record.Reference.Page))
            using (var headersPage = _physicalPageManager.GetRecordAccessor<HeapHeader>(_headersPage))
            {
                
                if (page == null)
                    return;
                if (pageInfo.RegisteredPageType != RegisteredPageType)
                    throw new InvalidOperationException($"The record does not belong to this page");
                page.FreeRecord(record);
                var curHeader = headersPage
                    .IterateRecords()                    
                    .First(k => k.Data.LogicalPageNum == pageInfo.Reference.PageNum);
                curHeader.Data.Fullness = pageInfo.PageFullness;
                headersPage.StoreRecord(curHeader);
            }
        }

        public TypedRecord<TRecord> GetRecord(PageRecordReference reference)
        {
            using (var pageInfo = _physicalPageManager.GetPageInfo(reference.Page))
            using (var page = _physicalPageManager.GetRecordAccessor<TRecord>(reference.Page))
            {
                if (page == null)
                    return null;
                if (pageInfo.RegisteredPageType != RegisteredPageType)
                    throw new InvalidOperationException($"The record does not belong to this page");
                return page.GetRecord(reference);
            }
        }

        public void StoreRecord(TypedRecord<TRecord> record)
        {
            using (var pageInfo = _physicalPageManager.GetPageInfo(record.Reference.Page))
            using (var page = _physicalPageManager.GetRecordAccessor<TRecord>(record.Reference.Page))
            {
                if (page == null)
                    return;
                if (pageInfo.RegisteredPageType != RegisteredPageType)
                    throw new InvalidOperationException($"The record does not belong to this page");
                page?.StoreRecord(record);   
            }
        }

        public IEnumerable<TypedRecord<TRecord>> IterateRecords()
        {
            using (var headersPage = _physicalPageManager.GetRecordAccessor<HeapHeader>(_headersPage))
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
