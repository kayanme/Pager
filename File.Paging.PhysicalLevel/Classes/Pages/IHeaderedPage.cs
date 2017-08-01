namespace Pager.Classes
{
    public interface IHeaderedPage:IPage
    {      
        IPage Content { get; }    
    }

        public interface IHeaderedPage<THeader>: IHeaderedPage where THeader : new()
    {       
        THeader GetHeader();
        void ModifyHeader(THeader header);
    }
}