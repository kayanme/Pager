using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.PageFactories;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;
using File.Paging.PhysicalLevel.Contracts;
using FIle.Paging.LogicalLevel.Classes.Configurations;
using FIle.Paging.LogicalLevel.Classes.Factories;
using FIle.Paging.LogicalLevel.Contracts;

namespace FIle.Paging.LogicalLevel.Classes
{
    [Export(typeof(ILogicalPageManager))]
    internal sealed class LogicalPageManager : ILogicalPageManager
    {
        private readonly IPageManager _physicalManager;
        private readonly LogicalPageManagerConfiguration _config;
        private readonly ILogicalPageFactory _pageFactory;

        [ImportingConstructor]
        public LogicalPageManager(IPageManager manager, LogicalPageManagerConfiguration config,
            ILogicalPageFactory pageFactory)
        {
            _physicalManager = manager;
            _config = config;
            _pageFactory = pageFactory;
        }

        public IHeaderedPage<THeader> GetHeaderAccessor<THeader>(PageReference pageNum) where THeader : new()
        {
            throw new NotImplementedException();
        }

        public IPageInfo GetPageInfo(PageReference pageNum)
        {
            throw new NotImplementedException();
        }

        public IPhysicalLocks GetPageLocks(PageReference pageNum)
        {
            throw new NotImplementedException();
        }

        public IPage<TRecord> GetRecordAccessor<TRecord>(PageReference pageNum) where TRecord : struct
        {
            LogicalPageConfiguration conf = null;
            if (pageNum is VirtualPageReference)
            {
                var t = (VirtualPageReference)pageNum;
                Debug.Assert(_config.Configuration.ContainsKey(t.PageType));
                var config = _config.Configuration[t.PageType];
                return _pageFactory.GetVirtualRecordAccessor<TRecord>(_physicalManager, config as VirtualPageConfiguration, t,
                    t.PageType);
            }
            using (var page = _physicalManager.GetPageInfo(pageNum))
            {

                if (_config.Configuration.ContainsKey(page.RegisteredPageType))
                    conf = _config.Configuration[page.RegisteredPageType];
            }
            if (conf != null)
                return _pageFactory.GetBindedRecordAccessor<TRecord>(_physicalManager,
                    conf as BindedToPhysicalPageConfiguration,
                    pageNum);
            else
                return _physicalManager.GetRecordAccessor<TRecord>(pageNum);
        }

        public IBinarySearcher<TRecord> GetBinarySearchForPage<TRecord>(PageReference pageNum) where TRecord : struct
        {
            throw new NotImplementedException();
        }

        public ILogicalRecordOrderManipulation GetSorter<TRecord>(PageReference pageNum) where TRecord : struct
        {
            throw new NotImplementedException();
        }

        public PageReference CreatePage(byte type)
        {
            LogicalPageConfiguration conf = null;
            if (_config.Configuration.ContainsKey(type))
                conf = _config.Configuration[type];

            return ReturnPageUponConfig(type, conf);

        }

        private PageReference ReturnPageUponConfig(byte type, LogicalPageConfiguration conf)
        {
            PageReference page;
            switch (conf)
            {
                case null:
                    page = _physicalManager.CreatePage(type);
                    return page;
                case BindedToPhysicalPageConfiguration config:
                    page = _physicalManager.CreatePage(type);
                    return page;
                case VirtualPageConfiguration config:
                    return _pageFactory.CreateVirtualPage(_physicalManager, config);
                default:
                    throw new NotImplementedException();
            }
        }

        public void DeletePage(PageReference page, bool ensureEmptyness)
        {
            if (page is VirtualPageReference)
                return;
            _physicalManager.DeletePage(page, ensureEmptyness);
        }

        public void RecreatePage(PageReference pageNum, byte type)
        {
            if (pageNum is VirtualPageReference)
                return;
            _physicalManager.RecreatePage(pageNum, type);
        }

        public IEnumerable<PageReference> IteratePages(byte pageType)
        {
            if (_config.Configuration.ContainsKey(pageType))
            {
                var c = _config.Configuration[pageType];
                if (c is VirtualPageConfiguration)
                    yield return _pageFactory.CreateVirtualPage(_physicalManager, (VirtualPageConfiguration) c);
                else
                {
                    foreach (var page in _physicalManager.IteratePages(pageType))
                    {
                        yield return page;
                    }
                }
            }
            else
                foreach (var page in _physicalManager.IteratePages(pageType))
                {
                    yield return page;
                }
        }


        public void Dispose()
        {
            _physicalManager.Dispose();
        }


       
    }
}


