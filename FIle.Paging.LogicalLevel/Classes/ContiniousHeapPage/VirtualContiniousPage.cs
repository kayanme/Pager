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
        private PageReference _theBestCandidate;
        public VirtualContiniousPage(IPageManager physicalPageManager, byte headerPageTypeNum, byte pageType)
        {
            _physicalPageManager = physicalPageManager;
            _headerPageTypeNum = headerPageTypeNum;
            RegisteredPageType = pageType;
            Reference = new VirtualPageReference(0,pageType);
            PageFullness = 0;

            _headersPage= (_physicalPageManager.IteratePages(headerPageTypeNum).FirstOrDefault()??
                                _physicalPageManager.CreatePage(headerPageTypeNum)) as IPage<HeapHeader>;

            FindOrCreateNewCandidate();
        }

        private void FindOrCreateNewCandidate()
        {
            
            var theBestCandidate = _headersPage.IterateRecords()
                .Where(k => k.Fullness < .95)
                .Select(k => new PageReference((int)k.LogicalPageNum))
                .FirstOrDefault();

            if (theBestCandidate == null)
            {
                var newPage = _physicalPageManager.CreatePage(RegisteredPageType);
                theBestCandidate = newPage.Reference;
                if (!_headersPage.AddRecord(new HeapHeader
                {
                    Fullness = 0,
                    LogicalPageNum = (uint) newPage.Reference.PageNum
                }))
                {
                    _physicalPageManager.DeletePage(newPage.Reference,false);
                    throw new InvalidOperationException("No more records allowed");
                }
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
            using (var page = _physicalPageManager.RetrievePage(_theBestCandidate) as IPage<TRecord>)
            {
                while (!page.AddRecord(type))
                {
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
            return true;
        }

        public void FreeRecord(TRecord record)
        {
            using (var page = _physicalPageManager.RetrievePage(record.Reference.Page) as IPage<TRecord>)
            {             
                page?.FreeRecord(record);
            }
        }

        public TRecord GetRecord(PageRecordReference reference)
        {
            using (var page = _physicalPageManager.RetrievePage(reference.Page) as IPage<TRecord>)
            {
               return page?.GetRecord(reference);
            }
        }

        public void StoreRecord(TRecord record)
        {
            using (var page = _physicalPageManager.RetrievePage(record.Reference.Page) as IPage<TRecord>)
            {
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
