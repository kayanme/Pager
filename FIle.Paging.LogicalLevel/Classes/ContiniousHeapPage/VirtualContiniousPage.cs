using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;

namespace FIle.Paging.LogicalLevel.Classes.ContiniousHeapPage
{
     internal sealed class VirtualContiniousPage<TRecord>:IPage<TRecord>
        where TRecord:TypedRecord,new()
         
    {
        private readonly IPageManager _physicalPageManager;
        private PageReference _headersPage;
        private HeapHeader _theBestCandidate;
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
                        .Select(headersPage.GetRecord)
                        .Where(k => k != null)
                        .FirstOrDefault(k => k.Fullness < .95);

                if (theBestCandidate == null)
                {
                    var newPage = _physicalPageManager.CreatePage(RegisteredPageType);
                    theBestCandidate = new HeapHeader
                    {
                        Fullness = 0,
                        LogicalPageNum = (uint) newPage.PageNum
                    };
                    if (!headersPage.AddRecord(theBestCandidate))
                    {
                        _physicalPageManager.DeletePage(newPage, false);
                        throw new InvalidOperationException("No more records allowed");
                    }
                 //   (_physicalPageManager as IPhysicalPageManipulation).Flush(_headersPage);
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
      
        public bool AddRecord(TRecord type)
        {                 
            bool wereAdded = false;
            using (var headersPage = _physicalPageManager.GetRecordAccessor<HeapHeader>(_headersPage))
            {
                while (!wereAdded)
                {
                    var currentCandidate = _theBestCandidate;
                    using (var page =
                        _physicalPageManager.GetRecordAccessor<TRecord>(
                            new PageReference((int) currentCandidate.LogicalPageNum)))
                    {
                        wereAdded = page.AddRecord(type);
                        if (wereAdded)
                        {
                            currentCandidate.Fullness = _physicalPageManager
                                .GetPageInfo(new PageReference((int) currentCandidate.LogicalPageNum))
                                .PageFullness;
                            headersPage.StoreRecord(currentCandidate);
                        }
                    }
                    if (!wereAdded)
                    {
                        currentCandidate.Fullness = 1;
                        headersPage.StoreRecord(currentCandidate);
                        try
                        {
                            FindOrCreateNewCandidate();

                        }
                        catch (InvalidOperationException)
                        {
                            return false;
                        }
                    }
                }
            }
         //   (_physicalPageManager as IPhysicalPageManipulation).Flush(_headersPage);
            return true;
        }

        public void FreeRecord(TRecord record)
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
                    .Select(headersPage.GetRecord)
                    .First(k => k.LogicalPageNum == pageInfo.Reference.PageNum);
                curHeader.Fullness = pageInfo.PageFullness;
                headersPage.StoreRecord(curHeader);
            }
        }

        public TRecord GetRecord(PageRecordReference reference)
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

        public void StoreRecord(TRecord record)
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

        public IEnumerable<PageRecordReference> IterateRecords()
        {
            using (var headersPage = _physicalPageManager.GetRecordAccessor<HeapHeader>(_headersPage))
            {
                foreach (var header in headersPage.IterateRecords().Select(headersPage.GetRecord))
                {
                    using (var page =
                        _physicalPageManager.GetRecordAccessor<TRecord>(new PageReference((int) header.LogicalPageNum)))
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

        public void Flush()
        {
            
        }
    }
}
