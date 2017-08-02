using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager;
using Pager.Classes;

namespace File.Paging.PhysicalLevel.MemoryStubs
{
    internal sealed class HeaderedPageStub<THeader> : IHeaderedPage<THeader> where THeader:new()
    {

    
        private IPage _childPage;
        public IPage Content => _childPage;
        public PageReference Reference { get; }

        public double PageFullness => _childPage.PageFullness;

        public byte RegisteredPageType => _childPage.RegisteredPageType;

        private HeaderPageConfiguration<THeader> _config;
        internal HeaderedPageStub(IPage childPage, PageReference reference, HeaderPageConfiguration<THeader> config)
        {
           
            _childPage = childPage;
            _config = config;
            Reference = reference;
          
        }

        

      

        public void Dispose()
        {
           
        }

        public void Flush()
        {
        
        }

        private THeader _header;
        public THeader GetHeader()
        {
            return _header;
        }

        public void ModifyHeader(THeader header)
        {
            _header = header;
        }
    }
}
