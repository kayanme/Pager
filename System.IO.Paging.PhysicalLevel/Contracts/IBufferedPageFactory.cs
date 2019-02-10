using System;
using System.Collections.Generic;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.Text;

namespace System.IO.Paging.PhysicalLevel.Contracts
{
    internal interface IBufferedPageFactory
    {
        BufferedPage CreateHeaderedPage(int pageNum, PageContentConfiguration pageConfig, PageHeadersConfiguration headerConfig, int _pageSize, int extentSize);
        BufferedPage CreatePage(int pageNum, PageContentConfiguration _config, int _pageSize, int extentSize);
    }
}
