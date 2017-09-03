namespace File.Paging.PhysicalLevel.Classes.Pages
{
   

    public interface IHeaderedPage<THeader>
    {       
        THeader GetHeader();
        void ModifyHeader(THeader header);
    }
}