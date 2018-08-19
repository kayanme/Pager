using System.IO.Paging.PhysicalLevel.Classes.References;

namespace System.IO.Paging.PhysicalLevel.Classes.Pages
{
    internal abstract class TypedPageBase :IDisposable
    {
      
        private bool _disposedValue = false;
        public  PageReference Reference { get; }
      
        private Action ActionToClean;
        
       
        protected TypedPageBase(PageReference reference,Action action)
        {
            ActionToClean = action;
            Reference = reference;
         
        }

     
        protected void CheckReferenceToPageAffinity(PageRecordReference reference)
        {
            if (reference.Page != Reference)
                throw new ArgumentException("The record is on another page");
        }
     
     

        protected void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {                                       
                  
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