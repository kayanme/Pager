namespace System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts
{
   

    public interface IHeaderedPage<THeader>
    {       
        THeader GetHeader();
        void ModifyHeader(THeader header);
    }
}