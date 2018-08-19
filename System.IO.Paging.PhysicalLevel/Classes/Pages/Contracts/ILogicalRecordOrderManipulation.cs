using System.IO.Paging.PhysicalLevel.Classes.References;

namespace System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts
{
    public interface ILogicalRecordOrderManipulation :IDisposable
    {        
        void ApplyOrder(PageRecordReference[] records);
        void DropOrder(PageRecordReference record);
      
    }
}