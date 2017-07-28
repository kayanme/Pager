using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager.Classes;
using Pager.Contracts;
using Pager.Implementations;

namespace Pager
{

    [Export]
    public class PageManagerConfiguration
    {

        public Dictionary<byte, PageConfiguration> PageMap = new Dictionary<byte, PageConfiguration>();

        public enum PageSize { Kb4 = 4*1024, Kb8 = 8 * 1024 }

        public PageSize SizeOfPage { get; set; }
    }

   

   

   
  

  
}
