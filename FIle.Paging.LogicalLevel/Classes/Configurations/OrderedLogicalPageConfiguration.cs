using System;
using System.Diagnostics;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace FIle.Paging.LogicalLevel.Classes.Configurations
{
    internal sealed class OrderedLogicalPageConfiguration<TRecord,TKey>:LogicalPageConfiguration where TRecord:TypedRecord,new() where TKey:IComparable<TKey>
    {
        public Func<TRecord, TKey> KeySelector;

        public override IPage CreateLogicalPage(IPage physicalPage)
        {
            switch (physicalPage)
            {
                case IHeaderedPage i :
                    var p2 = i;
                    var logic = new OrderedPage<TRecord, TKey>(p2.Content as IPage<TRecord>, KeySelector);
                    Debug.Assert(physicalPage is IHeaderedPageInt, "physicalPage is IHeaderedPageInt");
                    ((IHeaderedPageInt) physicalPage).SwapContent(logic);
                    return CreateTransactionPage(i);
                case IPage<TRecord> i:
                    var page = new OrderedPage<TRecord, TKey>(physicalPage as IPage<TRecord>, KeySelector);
                    return CreateTransactionPage(page);
                default: throw new NotImplementedException();
            }           
        }
    }
}