using System;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace File.Paging.PhysicalLevel.Classes.PageFactories
{
    internal interface IPageFactory
    {
        IHeaderedPage<THeader> GetHeaderAccessor<THeader>(BufferedPage page, PageReference pageNum, Action actionToClean) where THeader : new();
        IPage GetPageInfo(BufferedPage page, PageReference pageNum, Action actionToClean);
        IPhysicalLocks GetPageLocks(BufferedPage page, PageReference pageNum, Action actionToClean);
        IPage<TRecord> GetRecordAccessor<TRecord>(BufferedPage page, PageReference pageNum, Action actionToClean) where TRecord : TypedRecord, new();
        ILogicalRecordOrderManipulation GetSorter(BufferedPage page, PageReference pageNum, Action actionToClean);
    }
}