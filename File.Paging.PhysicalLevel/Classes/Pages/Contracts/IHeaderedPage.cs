namespace File.Paging.PhysicalLevel.Classes.Pages
{
    public interface IHeaderedPage:IPage
    {      
      
    }

    public interface IHeaderedPage<THeader>: IHeaderedPage where THeader : new()
    {       
        THeader GetHeader();
        void ModifyHeader(THeader header);
    }
}