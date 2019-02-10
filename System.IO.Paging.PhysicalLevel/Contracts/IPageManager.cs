using System.Collections.Generic;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;

namespace System.IO.Paging.PhysicalLevel.Contracts
{
    /// <summary>
    /// Page manipulation.
    /// </summary>
    public interface IPageManager:IDisposable
    {
        /// <summary>
        /// Get header accessor.
        /// </summary>
        /// <typeparam name="THeader">Type of a header.</typeparam>
        /// <param name="pageNum">Page reference.</param>
        /// <returns>Header accessor, if page were not deleted, or null</returns>
        /// <exception cref="Exceptions.NoAccessorAvailableException">If no headers configured for this page type</exception>
        /// <exception cref="Exceptions.RecordTypeDoesNotMatchesConfigurationException">If header type configured does not match the one requested.</exception>
        /// <exception cref="ObjectDisposedException">If page manager were disposed.</exception>
        IHeaderedPage<THeader> GetHeaderAccessor<THeader>(PageReference pageNum) where THeader : new();
        /// <summary>
        /// Retrieve page info.
        /// </summary>
        /// <param name="pageNum">Page reference.</param>
        /// <returns>Page information, if page exists, or null.</returns>
        /// <exception cref="ObjectDisposedException">If page manager were disposed.</exception>
        IPageInfo GetPageInfo(PageReference pageNum);
        /// <summary>
        /// Get lock accessor.
        /// </summary>
        /// <param name="pageNum">Page reference.</param>
        /// <returns>Lock manipulator, if page exists, or null.</returns>
        /// <exception cref="Exceptions.NoAccessorAvailableException">If no locks configured for this page type</exception>
        /// <exception cref="ObjectDisposedException">If page manager were disposed.</exception>
        IPhysicalLocks GetPageLocks(PageReference pageNum);
        IPage<TRecord> GetRecordAccessor<TRecord>( PageReference pageNum) where TRecord :struct;
        IBinarySearcher<TRecord> GetBinarySearchForPage<TRecord>(PageReference pageNum) where TRecord : struct;
        ILogicalRecordOrderManipulation GetSorter<TRecord>(PageReference pageNum) where TRecord : struct;

        PageReference CreatePage(byte type);
        void DeletePage(PageReference page);
        void RecreatePage(PageReference pageNum,byte type);
        IEnumerable<PageReference> IteratePages(byte pageType);
    }
}
