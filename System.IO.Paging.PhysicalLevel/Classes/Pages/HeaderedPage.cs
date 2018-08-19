using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;

namespace System.IO.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class HeaderedPage<THeader> :TypedPageBase, IHeaderedPage<THeader>      
        where THeader:new()
       
    {
        private readonly IPageAccessor _accessor;
     
     
     

        private readonly PageHeadersConfiguration<THeader> _config;
        

        internal HeaderedPage( IPageAccessor accessor,
            PageReference reference,PageHeadersConfiguration<THeader> config,
            Action action) 
            : base( reference,action)
        {
            _accessor = accessor;
        
            _config = config;
           
        }

        public THeader GetHeader()
        {          
            var header = new THeader();
            var bytes = _accessor.GetByteArray(0, _config.HeaderSize);
            _config.Header.FillFromBytes( bytes,ref header);
            return header;
         
        }

        public void ModifyHeader(THeader header)
        {
            var bytes = new byte[_config.Header.GetSize];
            _config.Header.FillBytes(ref header, bytes);
            _accessor.SetByteArray(bytes, 0, bytes.Length);
            _accessor.Flush();
        }
      

        ~HeaderedPage()
        {
            Dispose(true);
        }


     

      


    }
}
