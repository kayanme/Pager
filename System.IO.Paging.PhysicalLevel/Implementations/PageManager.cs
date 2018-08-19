using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.PageFactories;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.IO.Paging.PhysicalLevel.Events;
using System.Linq;
using System.Threading;

namespace System.IO.Paging.PhysicalLevel.Implementations
{
    [Export(typeof(IPageManager))]
    internal sealed class PageManager : IPageManager, IPhysicalPageManipulation
    {
       
        private readonly IExtentAccessorFactory _blockFactory;
        private readonly IGamAccessor _accessor;
        private readonly int _pageSize;
        private readonly PageManagerConfiguration _config;
        private readonly IUnderlyingFileOperator _operatorForDisposal;
        private readonly IPageFactory _pageFactory;
        private readonly IHeaderFactory _headerFactory;
        private ConcurrentDictionary<int, BufferedPage> _bufferedPages = new ConcurrentDictionary<int, BufferedPage>();
        private int _pages;

        [ImportingConstructor]
        internal PageManager(PageManagerConfiguration config,IGamAccessor accessor,
            IExtentAccessorFactory blockFactory,IUnderlyingFileOperator operatorForDisposal,
            IPageFactory pageFactory, IHeaderFactory headerFactory)
        {
          
            _accessor = accessor;
            _blockFactory = blockFactory;
            _config = config;
            
            _pageSize = config.SizeOfPage == PageManagerConfiguration.PageSize.Kb4 ? 4096 : 8192;
            accessor.InitializeGam((ushort)_pageSize);
            _operatorForDisposal = operatorForDisposal;
            _pageFactory = pageFactory;
            _headerFactory = headerFactory;

            _pages = (int)((operatorForDisposal.FileSize - Extent.Size) / _pageSize);            
        }

      


        public void DeletePage(PageReference page, bool ensureEmptyness)
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            _accessor.MarkPageFree(page.PageNum);
            Interlocked.Decrement(ref _pages);
        }

        public void RecreatePage(PageReference pageNum, byte pageType)
        {
            BufferedPage block;
            do
            {
                block = GetPageFromBuffer(pageNum);
            } while (Interlocked.CompareExchange(ref block.UserCount, -1, 0) != 0);
            Thread.BeginCriticalRegion();
            
            block.Accessor.ClearPage();
            _accessor.SetPageType(pageNum.PageNum, pageType);
            if (_firstPages.TryGetValue(block.PageType, out var _))
            {
                _firstPages.TryRemove(block.PageType, out var _);
            }

            Thread.EndCriticalRegion();
            RemovePageFromBuffer(pageNum);          
        }

        private readonly ConcurrentDictionary<byte,PageReference> _firstPages = new ConcurrentDictionary<byte, PageReference>();

        public IEnumerable<PageReference> IteratePages(byte pageType)
        {
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
            if (firstPage != null)
            {
                yield return firstPage;
                int pr = firstPage.PageNum + 1;

                foreach (var i in Enumerable.Range(pr, Extent.Size))
                {
                    if (_accessor.GetPageType(pr) == pageType)
                    {
                        var p = new PageReference(i);
                        yield return p;
                    }
                }

            }
        }

        public void RemovePageFromBuffer(PageReference page)
        {          
            if (_bufferedPages.TryRemove(page.PageNum, out var pg));
            {
                pg.Accessor.Dispose();                
                PageRemovedFromBuffer(this, new PageRemovedFromBufferEventArgs(page));
            }
        }

        public void MarkPageToRemoveFromBuffer(PageReference pageNum)
        {           
            _bufferedPages.TryGetValue(pageNum.PageNum, out var page);
            page.MarkedForRemoval = true;
        }

        public event PageRemovedFromBufferEventHandler PageRemovedFromBuffer = (_,__) => { };
        public event NewPageCreatedEventHandler PageCreated = (_,__)=>{};

        private void ReleasePageUseAndCleanIfNeeded(PageReference reference,BufferedPage bufferPage)
        {
            Interlocked.Decrement(ref bufferPage.UserCount);
            if (bufferPage.MarkedForRemoval)
            {
                if (Interlocked.CompareExchange(ref bufferPage.UserCount, -1, 0) == 0)
                {
                    RemovePageFromBuffer(reference);
                }
            }
        }

        private T RetrievePage<T>(PageReference pageNum,Func<IPageFactory,Func<BufferedPage,PageReference,Action,T>> factoryMethod) 
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            var page = GetPageFromBuffer(pageNum);
            T userPage;
            try
            {
                userPage = factoryMethod(_pageFactory)(page,pageNum, ()=>ReleasePageUseAndCleanIfNeeded(pageNum, page));                             
            }
            catch
            {
                Interlocked.Decrement(ref page.UserCount);
                throw;
            }

            if (userPage == null)            
                ReleasePageUseAndCleanIfNeeded(pageNum, page);
            return userPage;
        }

     


        private BufferedPage GetPageFromBuffer(PageReference pageNum)
        {
            BufferedPage page;
            int userCount;
            do
            {
                page = _bufferedPages.GetOrAdd(pageNum.PageNum, i =>
                {
                    var block = _blockFactory.GetAccessor(_accessor.GamShift(pageNum.PageNum) + i * _pageSize, _pageSize);
                    var pageType = _accessor.GetPageType(pageNum.PageNum);
                    var headerType = _config.HeaderConfig.ContainsKey(pageType) ? _config.HeaderConfig[pageType] : null;
                  
                    if (headerType == null)
                    {
                        if (!_config.PageMap.ContainsKey(pageType))
                            throw new InvalidOperationException("Unknown page type "+pageType);
                        var type = _config.PageMap[pageType];
                        var headers = _headerFactory.CreateHeaders(type, block, headerType);
                        return 
                        new BufferedPage
                        {
                            Accessor = block,
                            ContentAccessor = block,
                            Headers = headers,
                            Config = type,
                            PageType = pageType
                        };
                    }
                    else
                    {
                        var type = headerType.InnerPageMap;
                        if (type == null)
                            throw new InvalidOperationException();
                        var headers = _headerFactory.CreateHeaders(type,block,headerType);

                        return new BufferedPage
                        {
                            Accessor = block,
                            ContentAccessor = block.GetChildAccessorWithStartShift(headerType.HeaderSize),
                            Headers = headers,
                            Config = type,
                            HeaderConfig = headerType,
                            PageType = pageType
                        };
                    }
                });
               userCount = page.UserCount;
               
            } while (userCount == -1 || Interlocked.CompareExchange(ref page.UserCount, userCount+1, userCount) != userCount);
            return page;
        }

        public void Flush(params PageReference[] pages)
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            foreach (var t in  pages.Select(k=>_bufferedPages[k.PageNum]).GroupBy(k=>k.Accessor.ExtentNumber).Select(k=>k.First()))
            {
                t.Accessor.Flush();
            }
        }


        public IHeaderedPage<THeader> GetHeaderAccessor<THeader>(PageReference pageNum) where THeader : new()
        {
            return RetrievePage<IHeaderedPage<THeader>>(pageNum, k => k.GetHeaderAccessor<THeader>);
        }

        public IPageInfo GetPageInfo(PageReference pageNum)
        {
            return RetrievePage<IPageInfo>(pageNum, k => k.GetPageInfo);
        }

        public IPhysicalLocks GetPageLocks(PageReference pageNum)
        {
            return RetrievePage<IPhysicalLocks>(pageNum, k => k.GetPageLocks);
        }

        public IPage<TRecord> GetRecordAccessor<TRecord>(PageReference pageNum) where TRecord : struct
        {
            return RetrievePage<IPage<TRecord>>(pageNum, k => k.GetRecordAccessor<TRecord>);
        }

        public IBinarySearcher<TRecord> GetBinarySearchForPage<TRecord>(PageReference pageNum) where TRecord : struct
        {
            return RetrievePage<IBinarySearcher<TRecord>>(pageNum, k => k.GetBinarySearcher<TRecord>);
        }

        public ILogicalRecordOrderManipulation GetSorter<TRecord>(PageReference pageNum) where TRecord : struct
        {
            return RetrievePage<ILogicalRecordOrderManipulation>(pageNum, k => k.GetSorter<TRecord>);
        }

        public PageReference CreatePage(byte type)
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            if (type == 0)
                throw new ArgumentException("TRecordType");
            var newPageNum = _accessor.MarkPageUsed(type);
            Interlocked.Increment(ref _pages);
            PageCreated(this,new NewPageCreatedEventArgs(new PageReference(newPageNum),type));
            return new PageReference(newPageNum);
        }      


        private bool _disposedValue;

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {

                if (disposing)
                {
                    foreach (var p in _bufferedPages)
                    {
                        p.Value.Accessor.Dispose();
                    }
                    _accessor.Dispose();
                    _operatorForDisposal.Dispose();
                    _bufferedPages = null;                    
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
