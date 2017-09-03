using System;
using System.Collections.Generic;
using System.Linq;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.MemoryStubs
{
    internal sealed class PageManagerStub : IPageManager
    {
        private readonly Dictionary<PageReference, IPage> _pages = new Dictionary<PageReference, IPage>();
        private readonly Dictionary<PageReference, object> _headeredPages = new Dictionary<PageReference, object>();
        private readonly Dictionary<PageReference, byte> _pageTypes = new Dictionary<PageReference, byte>();
        private readonly PageManagerConfiguration _config;
        private readonly int _size;
        internal PageManagerStub(PageManagerConfiguration config)
        {
            _config = config;
            _size = config.SizeOfPage == PageManagerConfiguration.PageSize.Kb8 ? 8192 : 4096;
        }

        public IHeaderedPage<THeader> GetHeaderAccessor<THeader>(PageReference pageNum) where THeader : new()
        {
            lock (_pages)
            {
                if (!_headeredPages.ContainsKey(pageNum))
                {
                    var page = (IHeaderedPage<THeader>)_headeredPages[pageNum];
                    return page;
                }
                else return null;

            }
        }

        public IPage GetPageInfo(PageReference pageNum)
        {
            lock (_pages)
            {
                if (!_headeredPages.ContainsKey(pageNum))
                {
                    var page = _pages[pageNum];
                    return page;
                }
                else return null;

            }
        }

        public IPhysicalLocks GetPageLocks(PageReference pageNum)
        {
            throw new NotImplementedException();
        }

        public IPage<TRecord> GetRecordAccessor<TRecord>(PageReference pageNum) where TRecord : TypedRecord, new()
        {
            lock (_pages)
            {
                if (!_headeredPages.ContainsKey(pageNum))
                {
                    var page = _pages[pageNum] as IPage<TRecord>;
                    return page;
                }
                else return null;

            }
        }

        public ILogicalRecordOrderManipulation GetSorter(PageReference pageNum)
        {
            throw new NotImplementedException();
        }

        public PageReference CreatePage(byte type)
        {
            lock (_pages)
            {
                var headerConfig = _config.HeaderConfig.ContainsKey(type) ? _config.HeaderConfig[type] : null;
                var pageConfig = _config.PageMap[type];
               
                for (int i = 0; i < int.MaxValue; i++)
                {
                    var r = new PageReference (i);

                    if (!_pages.ContainsKey(r))
                    {
                        var page = Activator.CreateInstance(typeof(PageStub<>).MakeGenericType(pageConfig.RecordType), r, pageConfig, _size) as IPage;
                        _pageTypes.Add(r, type);
                        if (headerConfig == null)
                        {
                            _pages.Add(r, page);
                           
                            return r;
                        }
                        else
                        {
                            var hp = Activator.CreateInstance(typeof(HeaderedPageStub<,>).MakeGenericType(headerConfig.GetType().GetGenericArguments()), page, r, pageConfig);
                            _headeredPages.Add(r, hp);
                            return r;
                        }
                    }
                }
                throw new InvalidOperationException();
              
            }

        }

        public void DeletePage(PageReference page, bool ensureEmptyness)
        {
            lock (_pages)
            {
                if (_pageTypes.ContainsKey(page))
                    _pageTypes.Remove(page);
                if (_pages.ContainsKey(page))
                {
                    _pages.Remove(page);
                   
                }
                if (_headeredPages.ContainsKey(page))
                {
                    _headeredPages.Remove(page);
                }
            }
        }

        public void RecreatePage(PageReference pageNum, byte type)
        {
            lock (_pages)
            {
                if (_pages.ContainsKey(pageNum))
                {
                    _pages.Remove(pageNum);
                    _pageTypes[pageNum] = type;
                }
                var headerConfig = _config.HeaderConfig.ContainsKey(type) ? _config.HeaderConfig[type] : null;
                var pageConfig = _config.PageMap[type];
                var page = Activator.CreateInstance(typeof(PageStub<>).MakeGenericType(pageConfig.RecordType), pageNum, pageConfig, _size) as IPage;            
                if (headerConfig == null)
                {
                    _pages[pageNum]  = page;                   
                }
                else
                {
                    var hp = Activator.CreateInstance(typeof(HeaderedPageStub<,>).MakeGenericType(headerConfig.GetType().GetGenericArguments()), page, pageNum, pageConfig);
                    _headeredPages[pageNum] = hp;                 
                }
            }
        }

        public IEnumerable<PageReference> IteratePages(byte pageType)
        {

            foreach (var pageReference in _pageTypes.Where(k => k.Value == pageType).Select(k => k.Key))
            {
                yield return pageReference;
            }
        }

        public void Dispose()
        {
          
        }

      

    }
}
