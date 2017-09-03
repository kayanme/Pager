using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal class PageInfo: TypedPageBase,IPage
    {
        private IPageHeaders _headers;
        private readonly ushort _pageSize;
        private readonly uint _extent;

        public PageInfo(PageReference pageRef, 
            IPageHeaders headers, byte registeredPageType,
            ushort pageSize, uint extent,Action action)
            :base(pageRef, action)
        {
            _headers = headers;
            _pageSize = pageSize;
            _extent = extent;
            RegisteredPageType = registeredPageType;
        }
        public byte RegisteredPageType { get; }

        public double PageFullness => (double)_headers.TotalUsedSize / _pageSize;
        public int UsedRecords => _headers.RecordCount;
        public int ExtentNumber =>(int) _extent;
    }
}
