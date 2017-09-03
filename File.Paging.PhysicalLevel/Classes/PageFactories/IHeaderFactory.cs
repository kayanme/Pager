using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.PageFactories
{
    internal interface IHeaderFactory
    {
        IPageHeaders CreateHeaders(PageContentConfiguration config,
            IPageAccessor accessor, PageHeadersConfiguration headerConfig = null);
    }
}