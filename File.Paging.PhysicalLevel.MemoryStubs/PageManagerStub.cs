using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager;
using Pager.Classes;
using Pager.Exceptions;

namespace File.Paging.PhysicalLevel.MemoryStubs
{
    internal sealed class PageManagerStub : IPageManager
    {
        private Dictionary<PageReference, IPage> _pages = new Dictionary<PageReference, IPage>();
        private Dictionary<PageReference, object> _headeredPages = new Dictionary<PageReference, object>();
        private Dictionary<PageReference, byte> _pageTypes = new Dictionary<PageReference, byte>();
        private PageManagerConfiguration _config;
        private int _size;
        internal PageManagerStub(PageManagerConfiguration config)
        {
            _config = config;
            _size = config.SizeOfPage == PageManagerConfiguration.PageSize.Kb8 ? 8192 : 4096;
        }

        public IPage CreatePage(byte type)
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
                           
                            return page;
                        }
                        else
                        {
                            var hp = Activator.CreateInstance(typeof(HeaderedPageStub<>).MakeGenericType(headerConfig.GetType().GetGenericArguments()[0]), page, r, pageConfig) as IHeaderedPage;
                            _headeredPages.Add(r, hp);
                            return hp;
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
                if (_pages.ContainsKey(page))
                {
                    _pages.Remove(page);
                    _pageTypes.Remove(page);
                }
            }
        }

        public void Dispose()
        {
          
        }

        public void GroupFlush(params IPage[] pages)
        {
            
        }
      

        public IPage RetrievePage(PageReference pageNum)
        {
            lock (_pages)
            {
                if (!_pages.ContainsKey(pageNum))
                {
                    var page =_pages[pageNum];                  
                    return page;
                }
                else return null;

            }
        }
    }
}
