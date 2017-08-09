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
        private readonly byte _headerPageTypeNum;
        private IPage<HeapHeader> _headersPage;
        private HeapHeader _theBestCandidate;
        public VirtualContiniousPage(IPageManager physicalPageManager,  byte pageType, byte headerPageTypeNum)
        {
            _physicalPageManager = physicalPageManager;
            _headerPageTypeNum = headerPageTypeNum;
            RegisteredPageType = pageType;
            Reference = new VirtualPageReference(0,pageType);
            PageFullness = 0;

            var hp = (_physicalPageManager.IteratePages(headerPageTypeNum).FirstOrDefault()??
                                _physicalPageManager.CreatePage(headerPageTypeNum));
            _headersPage = hp as IPage<HeapHeader>;
            FindOrCreateNewCandidate();
        }

        private void FindOrCreateNewCandidate()
        {
            
            var theBestCandidate = _headersPage
                .IterateRecords()               
                .FirstOrDefault(k => k.Fullness < .95);

            if (theBestCandidate == null)
            {
                var newPage = _physicalPageManager.CreatePage(RegisteredPageType);
                theBestCandidate =  new HeapHeader
                {
                    Fullness = 0,
                    LogicalPageNum = (uint)newPage.Reference.PageNum
                };
                if (!_headersPage.AddRecord(theBestCandidate))
                {
                    _physicalPageManager.DeletePage(newPage.Reference,false);
                    throw new InvalidOperationException("No more records allowed");
                }
                (_headersPage as IPhysicalLevelManipulation).Flush();
            }
            _theBestCandidate = theBestCandidate;
        }

        public void Dispose()
        {
           _headersPage.Dispose();
        }

        public byte RegisteredPageType { get; }
        public PageReference Reference { get; }
        public double PageFullness { get; }

        public bool AddRecord(TRecord type)
        {
         
         
            bool wereAdded = false;
            while (!wereAdded)
            {
                var currentCandidate = _theBestCandidate;
                using (var page =
                    _physicalPageManager.RetrievePage(
                        new PageReference((int) currentCandidate.LogicalPageNum)) as IPage<TRecord>)
                {
                    wereAdded = page.AddRecord(type);
                    if (wereAdded)
                    {
                        currentCandidate.Fullness = page.PageFullness;
                        _headersPage.StoreRecord(currentCandidate);
                    }
                }
                if (!wereAdded)
                {
                    currentCandidate.Fullness = 1;
                    _headersPage.StoreRecord(currentCandidate);
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

            //    (_headersPage as IPhysicalLevelManipulation).Flush();
            return true;
        }

        public void FreeRecord(TRecord record)
        {
            using (var page = _physicalPageManager.RetrievePage(record.Reference.Page) as IPage<TRecord>)
            {
                if (page == null)
                    return;
                if (page.RegisteredPageType != RegisteredPageType)
                    throw new InvalidOperationException($"The record does not belong to this page");
                page.FreeRecord(record);
                var curHeader = _headersPage
                    .IterateRecords()
                    .First(k => k.LogicalPageNum == page.Reference.PageNum);
                curHeader.Fullness = page.PageFullness;
                _headersPage.StoreRecord(curHeader);             
            }
        }

        public TRecord GetRecord(PageRecordReference reference)
        {
            using (var page = _physicalPageManager.RetrievePage(reference.Page) as IPage<TRecord>)
            {
                if (page == null)
                    return null;
                if (page.RegisteredPageType != RegisteredPageType)
                    throw new InvalidOperationException($"The record does not belong to this page");
                return page.GetRecord(reference);
            }
        }

        public void StoreRecord(TRecord record)
        {
            using (var page = _physicalPageManager.RetrievePage(record.Reference.Page) as IPage<TRecord>)
            {
                if (page == null)
                    return;
                if (page.RegisteredPageType != RegisteredPageType)
                    throw new InvalidOperationException($"The record does not belong to this page");
                page?.StoreRecord(record);   
            }
        }

        public IEnumerable<TRecord> IterateRecords()
        {
            foreach (var header in _headersPage.IterateRecords())
            {
                using (var page = _physicalPageManager.RetrievePage(new PageReference((int) header.LogicalPageNum)) as IPage<TRecord>)
                {
                    if (page!=null)
                    foreach (var record in page.IterateRecords())
                    {
                        yield return record;
                    }
                }
            }
        }
    }
}
