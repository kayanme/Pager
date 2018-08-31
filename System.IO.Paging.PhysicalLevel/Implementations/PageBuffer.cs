using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.PageFactories;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.IO.Paging.PhysicalLevel.Events;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.IO.Paging.PhysicalLevel.Implementations
{
    [Export(typeof(IPageBuffer))]
    internal sealed class PageBuffer : IPageBuffer
    {
      
        private ConcurrentDictionary<int, BufferedPage> _bufferedPages = new ConcurrentDictionary<int, BufferedPage>();
        private readonly IGamAccessor _accessor;
        private readonly IBufferedPageFactory _pageFactory;
        private bool _disposedValue;
        [ImportingConstructor]
        public PageBuffer(IBufferedPageFactory pageFactory, IGamAccessor accessor)
        {
            _pageFactory = pageFactory;
            _accessor = accessor;
        }

        public event PageRemovedFromBufferEventHandler PageRemovedFromBuffer = (_, __) => { };
        public event NewPageCreatedEventHandler PageCreated = (_, __) => { };


        public void Flush(params PageReference[] pages)
        {
            if (_disposedValue)
                throw new ObjectDisposedException("IPageManager");
            foreach (var t in pages.Select(k => _bufferedPages[k.PageNum]).GroupBy(k => k.Accessor.ExtentNumber).Select(k => k.First()))
            {
                t.Accessor.Flush();
            }
        }

        public BufferedPage GetPageFromBuffer(PageReference pageNum,PageManagerConfiguration _config,int pageSize)
        {
            BufferedPage page;
            int userCount;
            do
            {
                page = _bufferedPages.GetOrAdd(pageNum.PageNum, i =>
                {

                    var pageType = _accessor.GetPageType(pageNum.PageNum);
                    if (!_config.PageMap.ContainsKey(pageType))
                        throw new InvalidOperationException("Unknown page type " + pageType);
                    var headerType = _config.HeaderConfig.ContainsKey(pageType) ? _config.HeaderConfig[pageType] : null;
                    var type = _config.PageMap[pageType];
                    var newPage = headerType == null ?
                            _pageFactory.CreatePage(pageNum.PageNum, type, pageSize)
                          : _pageFactory.CreateHeaderedPage(pageNum.PageNum, type, headerType, pageSize);
                    newPage.PageType = pageType;                    
                    return newPage;
                });
                userCount = page.UserCount;
                if (page.MarkedForRemoval)
                    RemovePageFromBuffer(pageNum);
            } while ((userCount == -1 || Interlocked.CompareExchange(ref page.UserCount, userCount + 1, userCount) != userCount || page.MarkedForRemoval));
            return page;
        }

        public void ReleasePageUseAndCleanIfNeeded(PageReference reference, BufferedPage bufferPage)
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

        public void RemovePageFromBuffer(PageReference page)
        {
            if (_bufferedPages.TryRemove(page.PageNum, out var pg)) ;
            {
                pg.Accessor.Dispose();
                PageRemovedFromBuffer(this, new PageRemovedFromBufferEventArgs(page));
            }
        }

        public void MarkPageToRemoveFromBuffer(PageReference pageNum)
        {
            _bufferedPages.TryGetValue(pageNum.PageNum, out var page);
            page.MarkedForRemoval = true;
            if (page.UserCount == 0)
                RemovePageFromBuffer(pageNum);
        }

        public void Dispose()
        {
            if (!_disposedValue)
            {
                foreach (var p in _bufferedPages)
                {
                    p.Value.Accessor.Dispose();
                }
                _accessor.Dispose();
              
                _bufferedPages = null;
                _disposedValue = true;
            }
        }
    }
}
