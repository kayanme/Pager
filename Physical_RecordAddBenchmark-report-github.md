``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 8.1 (6.3.9600)
Processor=Intel Core i7-4510U CPU 2.00GHz (Haswell), ProcessorCount=4
Frequency=2533195 Hz, Resolution=394.7584 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0
  Job-NQIHYO : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0

AnalyzeLaunchVariance=True  Runtime=Clr  InvocationCount=192  
LaunchCount=3  

```
 |             Method | PageSize |        WriteMethod |        Mean |      Error |      StdDev |      Median |
 |------------------- |--------- |------------------- |------------:|-----------:|------------:|------------:|
 |          **AddRecord** |      **Kb4** |          **FixedSize** |   **178.40 ns** |   **8.437 ns** |    **41.67 ns** |   **196.87 ns** |
 | AddRecordWithFlush |      Kb4 |          FixedSize | 5,877.09 ns | 218.740 ns | 1,132.24 ns | 5,387.36 ns |
 |          **AddRecord** |      **Kb4** | **FixedSizeWithOrder** |   **479.52 ns** |  **21.289 ns** |   **109.24 ns** |   **456.17 ns** |
 | AddRecordWithFlush |      Kb4 | FixedSizeWithOrder | 6,500.16 ns | 311.487 ns | 1,294.86 ns | 6,735.05 ns |
 |          **AddRecord** |      **Kb4** |              **Naive** |    **64.82 ns** |   **6.750 ns** |    **34.02 ns** |    **40.50 ns** |
 | AddRecordWithFlush |      Kb4 |              Naive | 5,937.25 ns | 297.323 ns | 1,312.85 ns | 5,188.19 ns |
 |          **AddRecord** |      **Kb8** |          **FixedSize** |   **245.72 ns** |  **22.136 ns** |    **50.86 ns** |   **220.92 ns** |
 | AddRecordWithFlush |      Kb8 |          FixedSize | 5,838.81 ns | 252.523 ns | 1,091.01 ns | 5,202.52 ns |
 |          **AddRecord** |      **Kb8** | **FixedSizeWithOrder** |   **833.20 ns** |  **39.398 ns** |   **171.89 ns** |   **730.59 ns** |
 | AddRecordWithFlush |      Kb8 | FixedSizeWithOrder | 7,191.34 ns | 330.955 ns | 1,319.50 ns | 7,687.30 ns |
 |          **AddRecord** |      **Kb8** |              **Naive** |    **84.05 ns** |   **9.541 ns** |    **48.27 ns** |    **63.46 ns** |
 | AddRecordWithFlush |      Kb8 |              Naive | 5,728.94 ns | 325.045 ns | 1,036.63 ns | 5,113.71 ns |
