using System;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class HeaderedPage<THeader> : IHeaderedPage<THeader>,IHeaderedPageInt where THeader:new()
    {
        private readonly IPageAccessor _accessor;
        public IPage Content { get; private set; }

        public PageReference Reference { get; }
        
        public double PageFullness => Content.PageFullness;

        public byte RegisteredPageType => Content.RegisteredPageType;

        private readonly PageHeadersConfiguration<THeader> _config;
        internal HeaderedPage(IPageAccessor accessor, IPage childPage, PageReference reference,PageHeadersConfiguration<THeader> config)
        {
            _accessor = accessor;
            Content = childPage;
            _config = config;
            Reference = reference;
        }

        public THeader GetHeader()
        {          
            var header = new THeader();
            var bytes = _accessor.GetByteArray(0, _config.Header.GetSize);
            _config.Header.FillFromBytes( bytes,header);
            return header;
         
        }

        public void ModifyHeader(THeader header)
        {
            var bytes = new byte[_config.Header.GetSize];
            _config.Header.FillBytes(header, bytes);
            _accessor.SetByteArray(bytes, 0, bytes.Length);
            _accessor.Flush();
        }     

        private bool _disposedValue = false;
        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _accessor.Flush();
                    Content.Dispose();
                }

                _disposedValue = true;
            }
        }
        ~HeaderedPage()
        {
            Dispose(true);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SwapContent(IPage page)
        {
            Content = page;
        }
    }
}
