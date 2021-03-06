﻿using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;

namespace System.IO.Paging.PhysicalLevel.MemoryStubs
{
    internal sealed class HeaderedPageStub<TRecord,THeader> : IHeaderedPage<THeader> where THeader:new()
        where TRecord:struct
    {
        public IPageInfo Content { get; }

        public PageReference Reference { get; }

        public double PageFullness => Content.PageFullness;
        public int UsedRecords
        {
            get { return Content.UsedRecords; }
        }

        public byte RegisteredPageType => Content.RegisteredPageType;

        private PageHeadersConfiguration<THeader> _config;
        public HeaderedPageStub(IPageInfo childPage, PageReference reference, PageHeadersConfiguration< THeader> config)
        {
           
            Content = childPage;
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
