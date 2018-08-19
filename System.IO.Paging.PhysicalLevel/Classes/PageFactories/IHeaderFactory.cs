using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;

namespace System.IO.Paging.PhysicalLevel.Classes.PageFactories
{
    internal interface IHeaderFactory
    {
        IPageHeaders CreateHeaders(PageContentConfiguration config,
            IPageAccessor accessor, PageHeadersConfiguration headerConfig = null);
    }
}