using System;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    public interface ILogicalRecordOrderManipulation :IDisposable
    {        
        void ApplyOrder(PageRecordReference[] records);
        void DropOrder(PageRecordReference record);
      
    }
}