using System;
using System.Diagnostics;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;
using File.Paging.PhysicalLevel.Contracts;
using FIle.Paging.LogicalLevel.Classes.ContiniousHeapPage;
using FIle.Paging.LogicalLevel.Classes.OrderedPage;

namespace FIle.Paging.LogicalLevel.Classes.Configurations
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