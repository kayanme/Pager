using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pager.Contracts;

namespace Pager
{
    public abstract class TypedPage:IDisposable
    {
        public abstract PageReference Reference { get; }
        public abstract double PageFullness { get; }
        public abstract void Flush();
        public abstract void Dispose();
    }
  
}
