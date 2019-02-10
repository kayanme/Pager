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
        private readonly int _extentSize;
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
           
            accessor.InitializeGam((ushort)_pageSize,(ushort)config.ExtentSize);
            _operatorForDisposal = operatorForDisposal;
            _pageBuffer = pageBuffer;
            _pageFactory = pageFactory;
            _pageBuffer.PageRemovedFromBuffer += PageRemovedFromBuffer;
            _pageBuffer.PageCreated += PageCreated;

            _pages = (int)((operatorForDisposal.FileSize - config.ExtentSize) / _pageSize);            
        }

        public void MarkPageToRemoveFromBuffer(PageReference pageNum)
        {
            _pageBuffer.MarkPageToRemoveFromBuffer(pageNum);
        }


        public void DeletePage(PageReference pageNum)
        {
            var act = Tracing.Tracer.StartActivity(new Activity($"Deleting page"),pageNum);
            
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            var pageType = _accessor.GetPageType(pageNum.PageNum);
            _accessor.MarkPageFree(pageNum.PageNum);
            _firstPages.TryRemove(pageType, out var _);
            Interlocked.Decrement(ref _pages);
            Tracing.Tracer.StopActivity(act,null);
        }

        public void RecreatePage(PageReference pageNum, byte pageType)
        {
            var act = Tracing.Tracer.StartActivity(new Activity($"Recreating page"), (pageNum,pageType));            
            
            BufferedPage block;
            do
            {
                block = _pageBuffer.GetPageFromBuffer(pageNum,_config,_pageSize,_extentSize);
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
            Tracing.Tracer.StopActivity(act,null);
        }

        

        public IEnumerable<PageReference> IteratePages(byte pageType)
        {
           Tracing.Tracer.Write("Starting iterating pages of type",pageType);
            var fpf = Tracing.Tracer.StartActivity(new Activity("First page search"), null);
            var firstPage = _firstPages.GetOrAdd(pageType, pt =>
            {
                int pr = 0;
                while (pr < _extentSize && _accessor.GetPageType(pr) != pt)
                {
                    pr++;
                }
                ;
                if (pr == _extentSize)
                    return null;
                return new PageReference(pr);
            });
            
            Tracing.Tracer.StopActivity(fpf,null);
            if (firstPage != null)
            {
                yield return firstPage;
                int pr = firstPage.PageNum + 1;

                foreach (var i in Enumerable.Range(pr, _extentSize))
                {
                    if (_accessor.GetPageType(pr) == pageType)
                    {
                        var p = new PageReference(i);
                        Tracing.Tracer.Write($"Returned page ",p);
                        yield return p;
                    }
                }

            }
        }

        private T RetrievePage<T>(PageReference pageNum,Func<IPageFactory,Func<BufferedPage,PageReference,Action,T>> factoryMethod) 
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            
            var act = Tracing.Tracer.StartActivity(new Activity($"Retrieving page"), pageNum);
            var bufferAct = Tracing.Tracer.StartActivity(new Activity($"Retrieving page  from buffer"), pageNum);
            var page = _pageBuffer.GetPageFromBuffer(pageNum,_config,_pageSize,_extentSize);
            Tracing.Tracer.StopActivity(act,null);
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
            Tracing.Tracer.StopActivity(act,null);
            
            return userPage;
        }




     

        public void Flush(params PageReference[] pages)
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            var act = Tracing.Tracer.StartActivity(new Activity($"Flushing page buffer"),null);           
            _pageBuffer.Flush();
            Tracing.Tracer.StopActivity(act,null);
        }


        public IHeaderedPage<THeader> GetHeaderAccessor<THeader>(PageReference pageNum) where THeader : new()
        {
            Tracing.Tracer.Write($"Get header accesor", pageNum);
            return RetrievePage<IHeaderedPage<THeader>>(pageNum, k => k.GetHeaderAccessor<THeader>);
        }

        public IPageInfo GetPageInfo(PageReference pageNum)
        {
            Tracing.Tracer.Write($"Get page info ", pageNum);            
            return RetrievePage<IPageInfo>(pageNum, k => k.GetPageInfo);
        }

        public IPhysicalLocks GetPageLocks(PageReference pageNum)
        {
            Tracing.Tracer.Write($"Get page locks ", pageNum);
            
            return RetrievePage<IPhysicalLocks>(pageNum, k => k.GetPageLocks);
        }

        public IPage<TRecord> GetRecordAccessor<TRecord>(PageReference pageNum) where TRecord : struct
        {
            Tracing.Tracer.Write($"Get  record accesor  ", pageNum);
            
            return RetrievePage<IPage<TRecord>>(pageNum, k => k.GetRecordAccessor<TRecord>);
        }

        public IBinarySearcher<TRecord> GetBinarySearchForPage<TRecord>(PageReference pageNum) where TRecord : struct
        {
            Tracing.Tracer.Write($"Get binary searcher ", pageNum);
            
            return RetrievePage<IBinarySearcher<TRecord>>(pageNum, k => k.GetBinarySearcher<TRecord>);
        }

        public ILogicalRecordOrderManipulation GetSorter<TRecord>(PageReference pageNum) where TRecord : struct
        {
            Tracing.Tracer.Write($"Get sorter ", pageNum);            
            return RetrievePage<ILogicalRecordOrderManipulation>(pageNum, k => k.GetSorter<TRecord>);
        }

        public PageReference CreatePage(byte type)
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            if (type == 0)
                throw new ArgumentException("TRecordType");
            var act = Tracing.Tracer.StartActivity(new Activity($"Creating page"),type);
            var sw = Stopwatch.StartNew();
            var newPageNum = _accessor.MarkPageUsed(type);
            var newRef = new PageReference(newPageNum);
            _firstPages.AddOrUpdate(type, newRef, (s, n) => n == null ? newRef : n);
            Interlocked.Increment(ref _pages);
            Tracing.Tracer.StopActivity(act,null);
          
            PageCreated(this,new NewPageCreatedEventArgs(newRef, type));
           
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
