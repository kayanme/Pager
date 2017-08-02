using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager;
using Pager.Classes;

namespace FIle.Paging.LogicalLevel.Classes.Configurations
{
    public abstract class LogicalPageConfiguration
    {
        public abstract IPage CreateLogicalPage(IPage physicalPage);   
    }

    public sealed class OrderedLogicalPageConfiguration<TRecord,TKey>:LogicalPageConfiguration where TRecord:TypedRecord,new() where TKey:IComparable<TKey>
    {
        public Func<TRecord, TKey> KeySelector;

        public override IPage CreateLogicalPage(IPage physicalPage)
        {
            Debug.Assert(physicalPage is IPage<TRecord>, "physicalPage is IPage<TRecord>");
            return new OrderedPage<TRecord, TKey>(physicalPage as IPage<TRecord>, KeySelector);
        }
    }
}
