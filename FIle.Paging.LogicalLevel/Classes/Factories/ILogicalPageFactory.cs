using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using FIle.Paging.LogicalLevel.Classes.Configurations;

namespace FIle.Paging.LogicalLevel.Classes.Factories
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