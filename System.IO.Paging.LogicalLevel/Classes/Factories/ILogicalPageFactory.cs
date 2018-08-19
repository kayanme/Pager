using System.IO.Paging.LogicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Contracts;

namespace System.IO.Paging.LogicalLevel.Classes.Factories
{
    internal interface ILogicalPageFactory
    {
        VirtualPageReference CreateVirtualPage(IPageManager pageManager,VirtualPageConfiguration pageConfig);

        IPage<TRecord> GetVirtualRecordAccessor<TRecord>(IPageManager pageManager, VirtualPageConfiguration pageConfig,
            VirtualPageReference pageNum,
            byte pageType) where TRecord : struct;
        IPage<TRecord> GetBindedRecordAccessor<TRecord>(IPageManager pageManager, BindedToPhysicalPageConfiguration pageConfig, PageReference page) where TRecord : struct;

        
    }
}