using System.IO.Paging.LogicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Contracts;

namespace System.IO.Paging.LogicalLevel.Classes.OrderedPage
{
    internal abstract class OrderedLogicalPageConfiguration<TRecord> : BindedToPhysicalPageConfiguration
        where TRecord : struct
    {
       
        public abstract IPage<TRecord> CreatePage(PageReference pageReference, IPageManager manager, SortStateContoller sort);
    }

    internal sealed class OrderedLogicalPageConfiguration<TRecord,TKey>: OrderedLogicalPageConfiguration<TRecord> where TRecord:struct where TKey:IComparable<TKey>
    {
        public Func<TRecord, TKey> KeySelector;


        public override IPage<TRecord> CreatePage(PageReference pageReference,IPageManager manager,SortStateContoller sort)
        {
            return new OrderedPage<TRecord,TKey>(pageReference,manager,KeySelector,sort);
        }

        
    }

  
}