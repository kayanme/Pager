using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using FIle.Paging.LogicalLevel.Classes.Configurations;
using FIle.Paging.LogicalLevel.Contracts;

namespace FIle.Paging.LogicalLevel.Classes
{
    
    internal sealed class LogicalPageManager : ILogicalPageManager
    {
        private readonly IPageManager _physicalManager;
        private readonly LogicalPageManagerConfiguration _config;
        public LogicalPageManager (IPageManager manager,LogicalPageManagerConfiguration config)
        {
            _physicalManager = manager;
            _config = config;
        }

        public IPage CreatePage(byte type)
        {
            LogicalPageConfiguration conf = null;
            if (_config.Configuration.ContainsKey(type))
                conf = _config.Configuration[type];
           
            return ReturnPageUponConfig(type, conf);
           
        }

        private IPage ReturnPageUponConfig(byte type, LogicalPageConfiguration conf)
        {
            IPage page;
            switch (conf)
            {
                case null:
                    page = _physicalManager.CreatePage(type);
                    return page;
                case BindedToPhysicalPageConfiguration config:
                    page = _physicalManager.CreatePage(type);
                    return config.CreateLogicalPage(page);
                case VirtualPageConfiguration config:
                    return config.CreateLogicalPage(_physicalManager);
                default:
                    throw new NotImplementedException();
            }
        }

        public void DeletePage(PageReference page, bool ensureEmptyness)
        {
            if (page is VirtualPageReference)
                return;
            _physicalManager.DeletePage(page,ensureEmptyness);
        }

        public void RecreatePage(PageReference pageNum, byte type)
        {
            if (pageNum is VirtualPageReference)
                return;
            _physicalManager.RecreatePage(pageNum,type);
        }

        public IEnumerable<IPage> IteratePages(byte pageType)
        {
            if (_config.Configuration.ContainsKey(pageType))
            {
                var c = _config.Configuration[pageType];
                if (c is VirtualPageConfiguration)
                    yield return ((VirtualPageConfiguration) c).CreateLogicalPage(_physicalManager);
                else
                {
                    foreach (var page in _physicalManager.IteratePages(pageType))
                    {
                        yield return ((BindedToPhysicalPageConfiguration) c).CreateLogicalPage(page);
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
       

        public IPage RetrievePage(PageReference pageNum)
        {
            LogicalPageConfiguration conf = null;
            if (pageNum is VirtualPageReference)
            {
                var t = (VirtualPageReference) pageNum;
                Debug.Assert(_config.Configuration.ContainsKey(t.PageType));
                var config = _config.Configuration[t.PageType];
                return ReturnPageUponConfig(t.PageType, config);
            }
            using (var page = _physicalManager.RetrievePage(pageNum))
            {
                if (_config.Configuration.ContainsKey(page.RegisteredPageType))
                    conf = _config.Configuration[page.RegisteredPageType];

                return ReturnPageUponConfig(page.RegisteredPageType, conf);
            }
        }
    }
}
