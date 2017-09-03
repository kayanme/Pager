using System;
using System.Diagnostics;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using FIle.Paging.LogicalLevel.Classes.ContiniousHeapPage;

namespace FIle.Paging.LogicalLevel.Classes.Configurations
{
    internal abstract class OrderedLogicalPageConfiguration<TRecord> : BindedToPhysicalPageConfiguration
        where TRecord : TypedRecord, new()
    {
        public abstract IPage<TRecord> CreatePage(IPage<TRecord> sourcePage, ILogicalRecordOrderManipulation manipulation);
    }

    internal sealed class OrderedLogicalPageConfiguration<TRecord,TKey>: OrderedLogicalPageConfiguration<TRecord> where TRecord:TypedRecord,new() where TKey:IComparable<TKey>
    {
        public Func<TRecord, TKey> KeySelector;


        public override IPage<TRecord> CreatePage(IPage<TRecord> sourcePage,ILogicalRecordOrderManipulation manipulation)
        {
            return new OrderedPage<TRecord,TKey>(sourcePage,manipulation,KeySelector);
        }
    }
}