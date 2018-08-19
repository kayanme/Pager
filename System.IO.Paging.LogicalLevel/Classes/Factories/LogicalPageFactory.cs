using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.IO.Paging.LogicalLevel.Classes.ContiniousHeapPage;
using System.IO.Paging.LogicalLevel.Classes.OrderedPage;
using System.IO.Paging.LogicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Contracts;

namespace System.IO.Paging.LogicalLevel.Classes.Factories
{
    [Export]
    internal sealed class LogicalPageFactory : ILogicalPageFactory
    {
        public VirtualPageReference CreateVirtualPage(IPageManager pageManager, VirtualPageConfiguration pageConfig)
        {
            return new VirtualPageReference(1,pageConfig.PageTypeNum);
        }

        public IPage<TRecord> GetVirtualRecordAccessor<TRecord>(IPageManager pageManager, VirtualPageConfiguration pageConfig,
            VirtualPageReference pageNum,
            byte pageType) where TRecord : struct
        {
            switch (pageConfig)
            {
                case ContiniousHeapPageConfiguration<TRecord> config:
                    return new VirtualContiniousPage<TRecord>(pageManager, pageType, config.HeaderPageType);
                default: return null;
            }          
        }

        private ConcurrentDictionary<PageReference, SortStateContoller> _sortStateContollers = new ConcurrentDictionary<PageReference, SortStateContoller>();
     

        public IPage<TRecord> GetBindedRecordAccessor<TRecord>(IPageManager pageManager, BindedToPhysicalPageConfiguration pageConfig,
            PageReference page) where TRecord : struct
        {
            switch (pageConfig)
            {
                case OrderedLogicalPageConfiguration<TRecord> config:
                   return config.CreatePage(page,pageManager, _sortStateContollers.GetOrAdd(page, (_) => new SortStateContoller()));
   
                   default: return null;
            }
           
        }


       

    }


}

