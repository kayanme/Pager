using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace System.IO.Paging.Diagnostics
{
    internal static class Tracing
    {
        internal static readonly DiagnosticSource Tracer = new DiagnosticListener("System.IO.Paging.Physical");
         
    }
}

