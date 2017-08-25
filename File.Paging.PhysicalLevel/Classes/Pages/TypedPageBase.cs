using System;
using System.Collections.Generic;
using System.Threading;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal abstract class TypedPageBase : IPhysicalRecordManipulation
    {
        protected readonly IPageAccessor Accessor;
        protected readonly IPageHeaders Headers;
        private bool _disposedValue = false;
        public  PageReference Reference { get; }
        public abstract double PageFullness { get; }
        public byte RegisteredPageType { get; }
        internal Action ActionToClean;
        
        protected readonly ReaderWriterLockSlim CompactionLock = new ReaderWriterLockSlim();

        protected TypedPageBase(IPageHeaders headers, IPageAccessor accessor, PageReference reference, byte pageType)
        {
            Headers = headers;
            Accessor = accessor;
            Reference = reference;
            RegisteredPageType = pageType;
        }

        public  void Flush()
        {
            Accessor.Flush();
        }
        protected void CheckReferenceToPageAffinity(PageRecordReference reference)
        {
            if (reference.Page != Reference)
                throw new ArgumentException("The record is on another page");
        }
     
        public void Compact()
        {
            try
            {
                CompactionLock.EnterWriteLock();
                Headers.Compact();
            }
            finally
            {
                CompactionLock.ExitWriteLock();
            }
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {                                       
                    CompactionLock.Dispose();
                }

                _disposedValue = true;
            }
        }

        ~TypedPageBase()
        {
            Dispose(true);
        }

       

        public void Dispose()
        {
            Dispose(true);
            ActionToClean();
            GC.SuppressFinalize(this);
        }
    
    }
}