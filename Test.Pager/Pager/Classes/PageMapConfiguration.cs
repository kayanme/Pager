using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager
{

    [Export]
//    [PartNotDiscoverable]
    public class PageMapConfiguration
    {

        public Dictionary<byte, Type> PageMap = new Dictionary<byte, Type>();

        public enum PageSize { Kb4 = 4*1024, Kb8 = 8 * 1024, }

        public PageSize SizeOfPage { get; set; }
    }
}
