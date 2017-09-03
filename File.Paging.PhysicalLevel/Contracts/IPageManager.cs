using System;
using System.Collections.Generic;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace File.Paging.PhysicalLevel.Contracts
{

    public interface IPageManager:IDisposable
    {
        IHeaderedPage<THeader> GetHeaderAccessor<THeader>(PageReference pageNum) where THeader : new();
        IPage GetPageInfo(PageReference pageNum);
        IPhysicalLocks GetPageLocks(PageReference pageNum);
        IPage<TRecord> GetRecordAccessor<TRecord>( PageReference pageNum) where TRecord : TypedRecord, new();
        ILogicalRecordOrderManipulation GetSorter(PageReference pageNum);

        PageReference CreatePage(byte type);
        void DeletePage(PageReference page, bool ensureEmptyness);
        void RecreatePage(PageReference pageNum,byte type);
        IEnumerable<PageReference> IteratePages(byte pageType);
    }
}
