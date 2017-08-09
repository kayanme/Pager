``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 8.1 (6.3.9600)
Processor=Intel Core i7-4510U CPU 2.00GHz (Haswell), ProcessorCount=4
Frequency=2533191 Hz, Resolution=394.7590 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0
  Job-NQIHYO : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0

AnalyzeLaunchVariance=True  Runtime=Clr  InvocationCount=192  
LaunchCount=3  

```
 |                  Method |  WriteMethod |       Mean |     Error |     StdDev |     Median |
 |------------------------ |------------- |-----------:|----------:|-----------:|-----------:|
 |               **AddRecord** |        **Naive** |   **6.273 us** | **0.2301 us** |  **1.1868 us** |   **6.075 us** |
 |      AddRecordWithFlush |        Naive |   6.736 us | 0.2530 us |  1.3094 us |   6.981 us |
 | AddRecordGroupWithFlush |        Naive | 288.742 us | 8.4995 us | 41.7383 us | 272.638 us |
 |               **AddRecord** | **VariableSize** |   **1.929 us** | **0.1005 us** |  **0.5083 us** |   **1.633 us** |
 |      AddRecordWithFlush | VariableSize |  73.751 us | 1.0750 us |  5.3888 us |  73.119 us |
 | AddRecordGroupWithFlush | VariableSize | 193.849 us | 3.6239 us | 18.0999 us | 188.349 us |
