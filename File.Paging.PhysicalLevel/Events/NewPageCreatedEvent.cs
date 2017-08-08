using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;

namespace File.Paging.PhysicalLevel.Events
{
    public sealed class NewPageCreatedEventArgs:EventArgs
    {
        public PageReference PageReference { get; }
        public byte Type { get; }

        internal NewPageCreatedEventArgs(PageReference pageReference,byte type)
        {
            PageReference = pageReference;
            Type = type;
        }
    }

    public delegate void NewPageCreatedEventHandler(object manager,NewPageCreatedEventArgs args);
}
