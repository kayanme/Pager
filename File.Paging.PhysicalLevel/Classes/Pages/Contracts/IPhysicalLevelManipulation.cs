using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    public interface IPhysicalRecordManipulation 
    {
        void Flush();      
        void Compact();                
    }
}
