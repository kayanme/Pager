using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Events;

namespace File.Paging.PhysicalLevel.Implementations
{
    [Export(typeof(IPageManager))]
    internal sealed class PageManager : IPageManager, IPagePhysicalManipulation
    {
       
        private readonly IExtentAccessorFactory _blockFactory;
        private readonly IGamAccessor _accessor;
        private readonly int _pageSize;
        private readonly PageManagerConfiguration _config;
        private readonly IUnderlyingFileOperator _operatorForDisposal;
        private ConcurrentDictionary<int, BufferedPage> _bufferedPages = new ConcurrentDictionary<int, BufferedPage>();
        private int _pages;

        [ImportingConstructor]
        internal PageManager(PageManagerConfiguration config,IGamAccessor accessor,
            IExtentAccessorFactory blockFactory,IUnderlyingFileOperator operatorForDisposal)
        {
          
            _accessor = accessor;
            _blockFactory = blockFactory;
            _config = config;
            
            _pageSize = config.SizeOfPage == PageManagerConfiguration.PageSize.Kb4 ? 4096 : 8192;
            _operatorForDisposal = operatorForDisposal;
            _pages = (int)((operatorForDisposal.FileSize - Extent.Size) / _pageSize);            
        }

      


        public async Task DeletePage(PageReference page, bool ensureEmptyness)
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            await _accessor.MarkPageFree(page.PageNum);
            Interlocked.Decrement(ref _pages);
        }

        public async Task RecreatePage(PageReference pageNum, byte pageType)
        {
            BufferedPage block;
            do
            {
                block = GetPageFromBuffer(pageNum);
            } while (Interlocked.CompareExchange(ref block.UserCount, -1, 0) != 0);
            Thread.BeginCriticalRegion();
            
            var t1 = block.Accessor.ClearPage();
            var t2 = _accessor.SetPageType(pageNum.PageNum, pageType);
            if (_firstPages.TryGetValue(block.PageType, out var _))
            {
                _firstPages.TryRemove(block.PageType, out var _);
            }
            await t1;
            await t2;

            Thread.EndCriticalRegion();
            RemovePageFromBuffer(pageNum);          
        }

        private readonly ConcurrentDictionary<byte,PageReference> _firstPages = new ConcurrentDictionary<byte, PageReference>();
        public IEnumerable<IPage> IteratePages(byte pageType)
        {
            var firstPage = _firstPages.GetOrAdd(pageType, pt =>
            {
                int pr = 0;
                while (pr < Extent.Size&&_accessor.GetPageType(pr).Result != pt)
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
                var p = RetrievePage(firstPage);
                if (p.RegisteredPageType == pageType)//page types can change while iterating, so we should make a check
                    yield return p;
                int pr = firstPage.PageNum+1;

                foreach (var i in Enumerable.Range(pr,Extent.Size))
                {
                    if (_accessor.GetPageType(pr).Result == pageType)
                    {
                        p = RetrievePage(new PageReference(i));
                        if (p.RegisteredPageType == pageType)
                            yield return p;
                    }
                }
                
            }
        }

        public void RemovePageFromBuffer(PageReference page)
        {          
            if (_bufferedPages.TryRemove(page.PageNum, out var _));
               PageRemovedFromBuffer(this,new PageRemovedFromBufferEventArgs(page));
        }

        public void MarkPageToRemoveFromBuffer(PageReference pageNum)
        {           
            _bufferedPages.TryGetValue(pageNum.PageNum, out var page);
            page.MarkedForRemoval = true;
        }

        public event PageRemovedFromBufferEventHandler PageRemovedFromBuffer = (_,__) => { };
        public event NewPageCreatedEventHandler PageCreated = (_,__)=>{};

        private void ReleasePageUseAndCleanIfNeeded(TypedPageBase page,BufferedPage bufferPage)
        {
            Interlocked.Decrement(ref bufferPage.UserCount);
            if (bufferPage.MarkedForRemoval)
            {
                if (Interlocked.CompareExchange(ref bufferPage.UserCount, -1, 0) == 0)
                {
                    RemovePageFromBuffer(page.Reference);
                }
            }
        }

        public async Task<IPage> RetrievePage(PageReference pageNum) 
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            var page = await GetPageFromBuffer(pageNum);

            try
            {
                if (page.HeaderConfig != null)
                {
                    var userPage = page.HeaderConfig.CreatePage(page.Headers, page.Accessor, pageNum, _pageSize,
                        page.PageType);
                    (userPage as TypedPageBase).ActionToClean =
                        () => ReleasePageUseAndCleanIfNeeded(userPage as TypedPageBase, page);

                    return userPage;
                }
                else
                {
                    var userPage = page.Config.CreatePage(page.Headers, page.Accessor, pageNum, _pageSize,
                        page.PageType);
                    (userPage as TypedPageBase).ActionToClean =
                        () => ReleasePageUseAndCleanIfNeeded(userPage as TypedPageBase, page);

                    return userPage;
                }
            }
            catch
            {
                Interlocked.Decrement(ref page.UserCount);
                throw;
            }
        }

        private async Task<BufferedPage> GetPageFromBuffer(PageReference pageNum)
        {
            BufferedPage page;
            int userCount;
            do
            {
                page = _bufferedPages.GetOrAdd(pageNum.PageNum, i =>
                {
                    var block =   _blockFactory.GetAccessor(Extent.Size + i * _pageSize, _pageSize);
                    var pageType = _accessor.GetPageType(pageNum.PageNum);
                    var headerType = _config.HeaderConfig.ContainsKey(pageType) ? _config.HeaderConfig[pageType] : null;
                    if (headerType == null)
                    {
                        var type = _config.PageMap[pageType];
                        var headers = type.CreateHeaders(block, 0);
                        return new BufferedPage {Accessor = block, Headers = headers, Config = type, PageType = pageType };
                    }
                    else
                    {
                        var type = headerType.InnerPageMap;
                        if (type == null)
                            throw new InvalidOperationException();
                        var headers = type.CreateHeaders(block, 0);

                        return new BufferedPage
                        {
                            Accessor = block,
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

        public void GroupFlush(params IPage[] pages)
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            foreach (var t in  pages.Select(k=>_bufferedPages[k.Reference.PageNum]).GroupBy(k=>k.Accessor.ExtentNumber).Select(k=>k.First()))
            {
                t.Accessor.Flush();
            }
        }


        public IPage CreatePage(byte type)
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            if (type == 0)
                throw new ArgumentException("TRecordType");
            var newPageNum = _accessor.MarkPageUsed(type);
            Interlocked.Increment(ref _pages);
            PageCreated(this,new NewPageCreatedEventArgs(new PageReference(newPageNum),type));
            return RetrievePage(new PageReference(newPageNum));
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
