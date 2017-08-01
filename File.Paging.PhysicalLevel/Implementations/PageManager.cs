using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pager.Classes;
using Pager.Implementations;

namespace Pager
{
    [Export(typeof(IPageManager))]
    internal sealed class PageManager : IPageManager
    {
       
        private IExtentAccessorFactory _blockFactory;
        private IGAMAccessor _accessor;
        private int _pageSize;
        private PageManagerConfiguration _config;
        private IUnderlyingFileOperator _operatorForDisposal;
        private ConcurrentDictionary<int, BufferedPage> _bufferedPages = new ConcurrentDictionary<int, BufferedPage>();
        private int _pages;

        [ImportingConstructor]
        internal PageManager(PageManagerConfiguration config,IGAMAccessor accessor,
            IExtentAccessorFactory blockFactory,IUnderlyingFileOperator operatorForDisposal)
        {
          
            _accessor = accessor;
            _blockFactory = blockFactory;
            _config = config;
            
            _pageSize = config.SizeOfPage == PageManagerConfiguration.PageSize.Kb4 ? 4096 : 8192;
            _operatorForDisposal = operatorForDisposal;
            _pages = (int)((operatorForDisposal.FileSize - Extent.Size) / _pageSize);            
        }

      


        public void DeletePage(PageReference page, bool ensureEmptyness)
        {
            if (disposedValue)
                throw new ObjectDisposedException("IPageManager");
            _accessor.MarkPageFree(page.PageNum);
            Interlocked.Decrement(ref _pages);
        }

   

        public IPage RetrievePage(PageReference pageNum) 
        {
            if (disposedValue)
                throw new ObjectDisposedException("IPageManager");
            var page = _bufferedPages.GetOrAdd(pageNum.PageNum, i =>
            {
                var block = _blockFactory.GetAccessor(Extent.Size + i * _pageSize, _pageSize);
                var pageType = _accessor.GetPageType(pageNum.PageNum);              
                var headerType =_config.HeaderConfig.ContainsKey(pageType)?_config.HeaderConfig[pageType] as HeaderPageConfiguration:null;
                if (headerType == null)
                {
                    var type = _config.PageMap[pageType];
                    var headers = type.CreateHeaders(block, 0);
                    return new BufferedPage { Accessor = block, Headers = headers, Config = type };
                }
                else
                {                  
                    var type = headerType.InnerPageMap;
                    if (type == null)
                        throw new InvalidOperationException();
                    var headers = type.CreateHeaders(block, 0);

                    return new BufferedPage { Accessor = block, Headers = headers, Config = type, HeaderConfig = headerType };
                }                                                                              
            });
            if (page.HeaderConfig != null)
               return (page.HeaderConfig as HeaderPageConfiguration).CreatePage(page.Headers, page.Accessor, pageNum, _pageSize);
            return page.Config.CreatePage(page.Headers,page.Accessor, pageNum,_pageSize);
        }

        public void GroupFlush(params IPage[] pages)
        {
            if (disposedValue)
                throw new ObjectDisposedException("IPageManager");
            foreach (var t in  pages.Select(k=>_bufferedPages[k.Reference.PageNum]).GroupBy(k=>k.Accessor.PageSize).Select(k=>k.First()))
            {
                t.Accessor.Flush();
            }
        }


        public IPage CreatePage(byte type)
        {
            if (disposedValue)
                throw new ObjectDisposedException("IPageManager");
            if (type == 0)
                throw new ArgumentException("TRecordType");
            var newPageNum = _accessor.MarkPageUsed(type);
            Interlocked.Increment(ref _pages);
            return RetrievePage(new PageReference(newPageNum));
        }      


        private bool disposedValue = false;
        void Dispose(bool disposing)
        {
            if (!disposedValue)
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

                disposedValue = true;
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
