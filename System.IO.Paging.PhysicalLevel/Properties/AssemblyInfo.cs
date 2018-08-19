using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
#if !NETSTANDARD2_0
[assembly: AssemblyTitle("Pager")]
[assembly: AssemblyProduct("Pager")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
#endif


[assembly: AssemblyCopyright("Copyright © Hewlett-Packard 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("48462091-cdbc-4e4e-aee7-f63665e15a64")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly:InternalsVisibleTo("Test.Pager")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Benchmark.Paging.PhysicalLevel")]
[assembly: InternalsVisibleTo("System.IO.Paging.LogicalLevel")]
[assembly: InternalsVisibleTo("System.IO.Paging.PhysicalLevel.MemoryStubs")]
[assembly: InternalsVisibleTo("Test.Paging.LogicalLevel")]
[assembly: InternalsVisibleTo("Test.Paging.PhysicalLevel")]