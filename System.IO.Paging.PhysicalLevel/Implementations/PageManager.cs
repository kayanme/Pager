using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.PageFactories;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.IO.Paging.Diagnostics;
using System.IO.Paging.PhysicalLevel.Events;
using System.Linq;
using System.Threading;

namespace System.IO.Paging.PhysicalLevel.Implementations
{
    [Export(typeof(IPageManager))]
    internal sealed class PageManager : IPageManager, IPhysicalPageManipulation
    {
         
        private readonly IGamAccessor _accessor;
        private readonly int _pageSize;
        private readonly PageManagerConfiguration _config;
        private readonly IUnderlyingFileOperator _operatorForDisposal;
        private readonly IPageBuffer _pageBuffer;
        private readonly IPageFactory _pageFactory;     
        private int _pages;
        private readonly ConcurrentDictionary<byte, PageReference> _firstPages = new ConcurrentDictionary<byte, PageReference>();
        public event PageRemovedFromBufferEventHandler PageRemovedFromBuffer = (_, __) => { };
        public event NewPageCreatedEventHandler PageCreated = (_, __) => { };


        [ImportingConstructor]
        internal PageManager(PageManagerConfiguration config,IGamAccessor accessor,
             IUnderlyingFileOperator operatorForDisposal,
            IPageFactory pageFactory, IPageBuffer pageBuffer)
        {
          
            _accessor = accessor;     
            _config = config;            
            _pageSize = config.SizeOfPage == PageManagerConfiguration.PageSize.Kb4 ? 4096 : 8192;
            accessor.InitializeGam((ushort)_pageSize);
            _operatorForDisposal = operatorForDisposal;
            _pageBuffer = pageBuffer;
            _pageFactory = pageFactory;
            _pageBuffer.PageRemovedFromBuffer += PageRemovedFromBuffer;
            _pageBuffer.PageCreated += PageCreated;

            _pages = (int)((operatorForDisposal.FileSize - Extent.Size) / _pageSize);            
        }

        public void MarkPageToRemoveFromBuffer(PageReference pageNum)
        {
            _pageBuffer.MarkPageToRemoveFromBuffer(pageNum);
        }


        public void DeletePage(PageReference pageNum, bool ensureEmptyness)
        {
            Tracing.Tracer.TraceInformation($"Deleting page {pageNum}");
            var sw = Stopwatch.StartNew();
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            var pageType = _accessor.GetPageType(pageNum.PageNum);
            _accessor.MarkPageFree(pageNum.PageNum);
            _firstPages.TryRemove(pageType, out var _);
            Interlocked.Decrement(ref _pages);
            Tracing.Tracer.TraceInformation($"Page {pageNum} deleted in {sw.Elapsed}");
        }

        public void RecreatePage(PageReference pageNum, byte pageType)
        {
            Tracing.Tracer.TraceInformation($"Recreating page {pageNum} as type {pageType}");
            var sw = Stopwatch.StartNew();
            BufferedPage block;
            do
            {
                block = _pageBuffer.GetPageFromBuffer(pageNum,_config,_pageSize);
            } while (Interlocked.CompareExchange(ref block.UserCount, -1, 0) != 0);

            Thread.BeginCriticalRegion();            
            block.Accessor.ClearPage();
            _accessor.SetPageType(pageNum.PageNum, pageType);
            if (_firstPages.TryGetValue(block.PageType, out var _))
            {
                _firstPages.TryRemove(block.PageType, out var _);
            }
            Thread.EndCriticalRegion();

            _pageBuffer.RemovePageFromBuffer(pageNum);
            Tracing.Tracer.TraceInformation($"Page {pageNum} recreated as {pageType} in {sw.Elapsed}");
        }

        

        public IEnumerable<PageReference> IteratePages(byte pageType)
        {
            Tracing.Tracer.TraceInformation($"Starting iterating pages of type {pageType}");
            var sw = Stopwatch.StartNew();
            var firstPage = _firstPages.GetOrAdd(pageType, pt =>
            {
                int pr = 0;
                while (pr < Extent.Size && _accessor.GetPageType(pr) != pt)
                {
                    pr++;
                }
                ;
                if (pr == Extent.Size)
                    return null;
                return new PageReference(pr);
            });
            Tracing.Tracer.TraceInformation($"First page found in {sw.Elapsed}");
            if (firstPage != null)
            {
                yield return firstPage;
                int pr = firstPage.PageNum + 1;

                foreach (var i in Enumerable.Range(pr, Extent.Size))
                {
                    if (_accessor.GetPageType(pr) == pageType)
                    {
                        var p = new PageReference(i);
                        Tracing.Tracer.TraceInformation($"Returned page {p}");
                        yield return p;
                    }
                }

            }
        }

        private T RetrievePage<T>(PageReference pageNum,Func<IPageFactory,Func<BufferedPage,PageReference,Action,T>> factoryMethod) 
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            Trace.Indent();
            var sw = Stopwatch.StartNew();
            var page = _pageBuffer.GetPageFromBuffer(pageNum,_config,_pageSize);
            Tracing.Tracer.TraceInformation($"Retrieving page {pageNum} from buffer in {sw.Elapsed}");
            T userPage;
            try
            {
                userPage = factoryMethod(_pageFactory)(page,pageNum, ()=> _pageBuffer.ReleasePageUseAndCleanIfNeeded(pageNum, page));                             
            }
            catch
            {
                Interlocked.Decrement(ref page.UserCount);
                throw;
            }
            
            if (userPage == null)
                _pageBuffer.ReleasePageUseAndCleanIfNeeded(pageNum, page);
            Tracing.Tracer.TraceInformation($"Retrieving page {pageNum} ended in {sw.Elapsed}");
            Trace.Unindent();
            return userPage;
        }




     

        public void Flush(params PageReference[] pages)
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            Tracing.Tracer.TraceInformation($"Flushing page buffer");
            var sw = Stopwatch.StartNew();
            _pageBuffer.Flush();
            Tracing.Tracer.TraceInformation($"Page buffer in {sw.Elapsed}");
        }


        public IHeaderedPage<THeader> GetHeaderAccessor<THeader>(PageReference pageNum) where THeader : new()
        {
            Tracing.Tracer.TraceInformation($"Get header accesor for {pageNum}");
            return RetrievePage<IHeaderedPage<THeader>>(pageNum, k => k.GetHeaderAccessor<THeader>);
        }

        public IPageInfo GetPageInfo(PageReference pageNum)
        {
            Tracing.Tracer.TraceInformation($"Get page info for {pageNum}");
            return RetrievePage<IPageInfo>(pageNum, k => k.GetPageInfo);
        }

        public IPhysicalLocks GetPageLocks(PageReference pageNum)
        {
            Tracing.Tracer.TraceInformation($"Get page locks for {pageNum}");
            return RetrievePage<IPhysicalLocks>(pageNum, k => k.GetPageLocks);
        }

        public IPage<TRecord> GetRecordAccessor<TRecord>(PageReference pageNum) where TRecord : struct
        {
            Tracing.Tracer.TraceInformation($"Get record accesor for {pageNum}");
            return RetrievePage<IPage<TRecord>>(pageNum, k => k.GetRecordAccessor<TRecord>);
        }

        public IBinarySearcher<TRecord> GetBinarySearchForPage<TRecord>(PageReference pageNum) where TRecord : struct
        {
            Tracing.Tracer.TraceInformation($"Get binary searcher for {pageNum}");
            return RetrievePage<IBinarySearcher<TRecord>>(pageNum, k => k.GetBinarySearcher<TRecord>);
        }

        public ILogicalRecordOrderManipulation GetSorter<TRecord>(PageReference pageNum) where TRecord : struct
        {
            Tracing.Tracer.TraceInformation($"Get sorter for {pageNum}");
            return RetrievePage<ILogicalRecordOrderManipulation>(pageNum, k => k.GetSorter<TRecord>);
        }

        public PageReference CreatePage(byte type)
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            if (type == 0)
                throw new ArgumentException("TRecordType");
            Tracing.Tracer.TraceInformation($"Creating page with type {type}");
            var sw = Stopwatch.StartNew();
            var newPageNum = _accessor.MarkPageUsed(type);
            var newRef = new PageReference(newPageNum);
            _firstPages.AddOrUpdate(type, newRef, (s, n) => n == null ? newRef : n);
            Interlocked.Increment(ref _pages);
            Tracing.Tracer.TraceInformation($"Page with type {type} created in {sw.Elapsed}");
            sw.Restart();
            PageCreated(this,new NewPageCreatedEventArgs(newRef, type));
            Tracing.Tracer.TraceInformation($"Page with type {type} creation event fired in {sw.Elapsed}");
            return new PageReference(newPageNum);
        }      


        private bool _disposedValue;

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {

                if (disposing)
                {
                    _pageBuffer.Dispose();
                    _accessor.Dispose();
                    _operatorForDisposal.Dispose();                                
                }

                _disposedValue = true;
            }
        }
        ~PageManager()
        {
            Dispose(true);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
