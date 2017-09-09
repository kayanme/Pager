using System;
using System.Collections.Generic;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;

namespace File.Paging.PhysicalLevel.Contracts
{

    public interface IPageManager:IDisposable
    {
        IHeaderedPage<THeader> GetHeaderAccessor<THeader>(PageReference pageNum) where THeader : new();
        IPageInfo GetPageInfo(PageReference pageNum);
        IPhysicalLocks GetPageLocks(PageReference pageNum);
        IPage<TRecord> GetRecordAccessor<TRecord>( PageReference pageNum) where TRecord :struct;
        IBinarySearcher<TRecord> GetBinarySearchForPage<TRecord>(PageReference pageNum) where TRecord : struct;
        ILogicalRecordOrderManipulation GetSorter<TRecord>(PageReference pageNum) where TRecord : struct;

        PageReference CreatePage(byte type);
        void DeletePage(PageReference page, bool ensureEmptyness);
        void RecreatePage(PageReference pageNum,byte type);
        IEnumerable<PageReference> IteratePages(byte pageType);
    }
}
