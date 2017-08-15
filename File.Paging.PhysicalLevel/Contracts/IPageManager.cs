using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace File.Paging.PhysicalLevel.Contracts
{

    public interface IPageManager:IDisposable
    {      

        Task<IPage> RetrievePage(PageReference pageNum);
        Task<IPage> CreatePage(byte type);
        Task DeletePage(PageReference page, bool ensureEmptyness);
        Task RecreatePage(PageReference pageNum,byte type);
        IEnumerable<IPage> IteratePages(byte pageType);
    }
}
