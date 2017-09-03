``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 8.1 (6.3.9600)
Processor=Intel Core i7-4510U CPU 2.00GHz (Haswell), ProcessorCount=4
Frequency=2533195 Hz, Resolution=394.7584 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0
  Job-NQIHYO : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0

AnalyzeLaunchVariance=True  Runtime=Clr  InvocationCount=192  
LaunchCount=3  

```
 |                  Method | WriteMethod |       Mean |      Error |     StdDev |     Median |
 |------------------------ |------------ |-----------:|-----------:|-----------:|-----------:|
 |               **AddRecord** |   **FixedSize** |   **1.417 us** |  **0.0147 us** |  **0.0372 us** |   **1.404 us** |
 |      AddRecordWithFlush |   FixedSize |  68.898 us |  1.3898 us |  5.5087 us |  67.339 us |
 | AddRecordGroupWithFlush |   FixedSize | 186.014 us |  5.7012 us | 29.3062 us | 172.375 us |
 |               **AddRecord** |       **Naive** |   **6.782 us** |  **0.3111 us** |  **1.4025 us** |   **7.338 us** |
 |      AddRecordWithFlush |       Naive |   6.588 us |  0.3926 us |  1.3452 us |   7.285 us |
 | AddRecordGroupWithFlush |       Naive | 290.726 us | 10.9190 us | 56.6159 us | 276.543 us |
