using System;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class HeaderedPage<THeader> : TypedPageBase, IHeaderedPage<THeader>,IHeaderedPageInt where THeader:new()
    {
     
        public IPage Content { get; private set; }

     
        
        public override double PageFullness => Content.PageFullness;

       

        private readonly PageHeadersConfiguration<THeader> _config;
        internal HeaderedPage(IPageHeaders childHeaders, IPageAccessor accessor, IPage childPage, PageReference reference,PageHeadersConfiguration<THeader> config)
            :base(childHeaders, accessor,reference,childPage.RegisteredPageType)
        {
          
            Content = childPage;
            _config = config;
           
        }

        public THeader GetHeader()
        {          
            var header = new THeader();
            var bytes = Accessor.GetByteArray(0, _config.Header.GetSize);
            _config.Header.FillFromBytes( bytes,header);
            return header;
         
        }

        public void ModifyHeader(THeader header)
        {
            var bytes = new byte[_config.Header.GetSize];
            _config.Header.FillBytes(header, bytes);
            Accessor.SetByteArray(bytes, 0, bytes.Length);
            Accessor.Flush();
        }
      

        ~HeaderedPage()
        {
            Dispose(true);
        }


     

        public void SwapContent(IPage page)
        {
            Content = page;
        }
    }
}
