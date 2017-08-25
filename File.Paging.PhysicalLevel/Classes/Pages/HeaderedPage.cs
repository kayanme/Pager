using System;
using System.Collections.Generic;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class HeaderedPage<TRecord,THeader> : TypedPageBase,IPage<TRecord>, IHeaderedPage<THeader>,IHeaderedPageInt<TRecord>        
        where THeader:new()
        where TRecord :TypedRecord, new()
    {

        private IPage<TRecord> _content;

     
        
        public override double PageFullness => _content.PageFullness;
        public int UsedRecords
        {
            get { return _pageImplementation.UsedRecords; }
        }


        private readonly PageHeadersConfiguration<TRecord, THeader> _config;
        private IPage<TRecord> _pageImplementation;

        internal HeaderedPage(IPageHeaders childHeaders, IPageAccessor accessor, IPage<TRecord> childPage, PageReference reference,PageHeadersConfiguration<TRecord,THeader> config)
            :base(childHeaders, accessor,reference,childPage.RegisteredPageType)
        {
          
            _content = childPage;
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


     

        public void SwapContent(IPage<TRecord> page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            _content = page;
        }

        public bool AddRecord(TRecord type)
        {
            return _content.AddRecord(type);
        }

        public void FreeRecord(TRecord record)
        {
           _content.FreeRecord(record);
        }

        public TRecord GetRecord(PageRecordReference reference)
        {
            return _content.GetRecord(reference);
        }

        public void StoreRecord(TRecord record)
        {
           _content.StoreRecord(record);
        }

        public IEnumerable<PageRecordReference> IterateRecords()
        {
            return _content.IterateRecords();
        }
    }
}
