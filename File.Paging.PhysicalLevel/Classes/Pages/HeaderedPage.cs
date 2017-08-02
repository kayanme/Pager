using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager.Classes
{
    internal sealed class HeaderedPage<THeader> : IHeaderedPage<THeader> where THeader:new()
    {
        private  IPageAccessor _accessor;
        private IPage _childPage;
        public IPage Content => _childPage;
        public PageReference Reference { get; }
        
        public double PageFullness => _childPage.PageFullness;

        public byte RegisteredPageType => _childPage.RegisteredPageType;

        private HeaderPageConfiguration<THeader> _config;
        internal HeaderedPage(IPageAccessor accessor, IPage childPage, PageReference reference,HeaderPageConfiguration<THeader> config)
        {
            _accessor = accessor;
            _childPage = childPage;
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
        }

        public  void Flush()
        {
            _accessor.Flush();
        }

        private bool disposedValue = false;
        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Flush();
                    _childPage.Dispose();
                }

                disposedValue = true;
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



    }
}
