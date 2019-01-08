using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace System.IO.Paging.Diagnostics
{
    public static class Tracing
    {
        public static readonly TraceSource Tracer = new TraceSource("System.IO.Paging.Physical");

        
    }
}
