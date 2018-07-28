``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i7-4510U CPU 2.00GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
Frequency=2533201 Hz, Resolution=394.7575 ns, Timer=TSC
.NET Core SDK=2.1.202
  [Host] : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT

AnalyzeLaunchVariance=True  Runtime=Core  InvocationCount=192  
LaunchCount=3  

```
|             Method | PageSize | WriteMethod |        Mean |       Error |     StdDev |       Median |
|------------------- |--------- |------------ |------------:|------------:|-----------:|-------------:|
|          **AddRecord** |      **Kb4** |   **FixedSize** |    **522.8 ns** |    **28.38 ns** |   **100.6 ns** |    **477.82 ns** |
| AddRecordWithFlush |      Kb4 |   FixedSize | 13,294.9 ns |   821.11 ns | 3,625.6 ns | 12,593.07 ns |
|          **AddRecord** |      **Kb4** |       **Naive** |    **179.4 ns** |    **26.55 ns** |   **136.2 ns** |     **87.28 ns** |
| AddRecordWithFlush |      Kb4 |       Naive | 16,245.0 ns | 1,143.69 ns | 5,562.2 ns | 12,740.90 ns |
|          **AddRecord** |      **Kb8** |   **FixedSize** |  **1,208.0 ns** |    **66.34 ns** |   **345.1 ns** |  **1,116.01 ns** |
| AddRecordWithFlush |      Kb8 |   FixedSize | 14,508.7 ns | 1,031.25 ns | 4,723.0 ns | 13,747.63 ns |
|          **AddRecord** |      **Kb8** |       **Naive** |    **332.2 ns** |    **47.19 ns** |   **242.6 ns** |    **212.08 ns** |
| AddRecordWithFlush |      Kb8 |       Naive | 14,029.9 ns | 1,117.95 ns | 4,982.9 ns | 12,453.47 ns |
