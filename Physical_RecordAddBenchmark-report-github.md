``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 8.1 (6.3.9600)
Processor=Intel Core i7-4510U CPU 2.00GHz (Haswell), ProcessorCount=4
Frequency=2533191 Hz, Resolution=394.7590 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0
  Job-NQIHYO : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0

AnalyzeLaunchVariance=True  Runtime=Clr  InvocationCount=192  
LaunchCount=3  

```
 |             Method | PageSize |  WriteMethod |        Mean |     Error |      StdDev |      Median |
 |------------------- |--------- |------------- |------------:|----------:|------------:|------------:|
 |          **AddRecord** |      **Kb4** |    **FixedSize** |   **373.01 ns** |  **19.41 ns** |    **97.28 ns** |   **401.85 ns** |
 | AddRecordWithFlush |      Kb4 |    FixedSize | 6,314.94 ns | 274.72 ns | 1,198.59 ns | 5,789.32 ns |
 |          **AddRecord** |      **Kb4** |        **Naive** |    **87.30 ns** |  **10.38 ns** |    **52.72 ns** |    **67.71 ns** |
 | AddRecordWithFlush |      Kb4 |        Naive | 6,212.81 ns | 257.15 ns | 1,321.87 ns | 5,654.63 ns |
 |          **AddRecord** |      **Kb4** | **VariableSize** |   **325.06 ns** |  **17.97 ns** |    **93.32 ns** |   **276.85 ns** |
 | AddRecordWithFlush |      Kb4 | VariableSize | 6,454.64 ns | 231.14 ns | 1,200.52 ns | 6,169.45 ns |
 |          **AddRecord** |      **Kb8** |    **FixedSize** |   **337.25 ns** |  **17.75 ns** |    **88.65 ns** |   **339.66 ns** |
 | AddRecordWithFlush |      Kb8 |    FixedSize | 6,291.58 ns | 233.97 ns | 1,208.96 ns | 6,038.69 ns |
 |          **AddRecord** |      **Kb8** |        **Naive** |    **82.03 ns** |  **10.35 ns** |    **52.53 ns** |    **64.25 ns** |
 | AddRecordWithFlush |      Kb8 |        Naive | 6,386.38 ns | 267.99 ns | 1,360.61 ns | 5,768.93 ns |
 |          **AddRecord** |      **Kb8** | **VariableSize** |   **313.50 ns** |  **16.81 ns** |    **85.80 ns** |   **260.64 ns** |
 | AddRecordWithFlush |      Kb8 | VariableSize | 6,807.07 ns | 233.77 ns | 1,216.28 ns | 6,962.77 ns |
