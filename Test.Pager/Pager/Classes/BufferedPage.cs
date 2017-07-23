using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager.Contracts;

namespace Pager.Classes
{
    internal class BufferedPage
    {
        public IPageAccessor Accessor;
        public IPageHeaders Headers;
    }
}
