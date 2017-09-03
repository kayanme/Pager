using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using FIle.Paging.LogicalLevel.Classes.Configurations;

namespace FIle.Paging.LogicalLevel.Classes.ContiniousHeapPage
{
    internal sealed class ContiniousHeapPageConfiguration<TRecord> : VirtualPageConfiguration
        where TRecord : TypedRecord, new() 
    {
        public byte HeaderPageType;

      
    }
}

