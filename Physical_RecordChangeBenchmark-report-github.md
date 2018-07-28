``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i7-4510U CPU 2.00GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
Frequency=2533201 Hz, Resolution=394.7575 ns, Timer=TSC
.NET Core SDK=2.1.202
  [Host] : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT

AnalyzeLaunchVariance=True  Runtime=Core  InvocationCount=192  
LaunchCount=3  

```
|                  Method | WriteMethod |       Mean |      Error |     StdDev |     Median |
|------------------------ |------------ |-----------:|-----------:|-----------:|-----------:|
|               **AddRecord** |   **FixedSize** |   **1.432 us** |  **0.1023 us** |  **0.4857 us** |   **1.246 us** |
|      AddRecordWithFlush |   FixedSize | 105.770 us |  1.7092 us |  8.3125 us | 104.167 us |
| AddRecordGroupWithFlush |   FixedSize | 198.105 us | 13.7089 us | 39.5534 us | 173.898 us |
|               **AddRecord** |       **Naive** |   **5.638 us** |  **0.0650 us** |  **0.1802 us** |   **5.569 us** |
|      AddRecordWithFlush |       Naive |   6.588 us |  0.4150 us |  1.5758 us |   5.788 us |
| AddRecordGroupWithFlush |       Naive | 291.662 us |  3.1502 us |  7.8451 us | 288.402 us |
