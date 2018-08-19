namespace System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts
{
    public interface IPhysicalRecordManipulation 
    {
        void Flush();      
        void Compact();                
    }
}
