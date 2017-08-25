using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    public interface IPhysicalRecordManipulation 
    {
        void Flush();      
        void Compact();                
    }


    public interface ILogicalRecordOrderManipulation
    {        
        void ApplyOrder(PageRecordReference[] records);
        void DropOrder(PageRecordReference record);
    }
}
