using System;
using System.Collections.Generic;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace File.Paging.PhysicalLevel.Contracts
{

    public interface IPageManager:IDisposable
    {      

        IPage RetrievePage(PageReference pageNum);      
        IPage CreatePage(byte type);
        void DeletePage(PageReference page, bool ensureEmptyness);
        void RecreatePage(PageReference pageNum,byte type);
        IEnumerable<IPage> IteratePages(byte pageType);
    }
}
