``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 8.1 (6.3.9600)
Processor=Intel Core i7-4510U CPU 2.00GHz (Haswell), ProcessorCount=4
Frequency=2533195 Hz, Resolution=394.7584 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0
  Job-YKXKBQ : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0

AnalyzeLaunchVariance=True  Runtime=Clr  InvocationCount=96  
LaunchCount=3  

```
 |                    Method | PageSize |       Mean |     Error |      StdDev |     Median |
 |-------------------------- |--------- |-----------:|----------:|------------:|-----------:|
 |        **AddRecordWithOrder** |      **Kb4** |   **721.7 ns** |  **26.29 ns** |   **135.38 ns** |   **662.9 ns** |
 |    AddRecordInVirtualPage |      Kb4 | 6,224.4 ns | 332.56 ns | 1,411.70 ns | 5,624.3 ns |
 |     AddRecordWithoutOrder |      Kb4 |   230.8 ns |  17.20 ns |    60.99 ns |   197.0 ns |
 |     ChangeRecordWithOrder |      Kb4 |         NA |        NA |          NA |         NA |
 | ChangeRecordInVirtualPage |      Kb4 | 6,689.9 ns | 637.19 ns | 3,309.55 ns | 6,575.2 ns |
 |  ChangeRecordWithoutOrder |      Kb4 | 1,689.6 ns |  57.32 ns |   218.34 ns | 1,734.7 ns |
 |        **AddRecordWithOrder** |      **Kb8** | **1,403.5 ns** |  **53.04 ns** |   **262.93 ns** | **1,411.8 ns** |
 |    AddRecordInVirtualPage |      Kb8 | 9,309.9 ns | 413.34 ns | 2,117.31 ns | 7,947.6 ns |
 |     AddRecordWithoutOrder |      Kb8 |   441.7 ns |  24.08 ns |   115.98 ns |   368.7 ns |
 |     ChangeRecordWithOrder |      Kb8 |         NA |        NA |          NA |         NA |
 | ChangeRecordInVirtualPage |      Kb8 | 4,416.5 ns | 299.09 ns |   962.46 ns | 3,942.6 ns |
 |  ChangeRecordWithoutOrder |      Kb8 | 1,240.7 ns |  82.61 ns |   356.92 ns | 1,007.7 ns |

Benchmarks with issues:
  RecordAddBenchmark.ChangeRecordWithOrder: Job-YKXKBQ(AnalyzeLaunchVariance=True, Runtime=Clr, InvocationCount=96, LaunchCount=3) [PageSize=Kb4]
  RecordAddBenchmark.ChangeRecordWithOrder: Job-YKXKBQ(AnalyzeLaunchVariance=True, Runtime=Clr, InvocationCount=96, LaunchCount=3) [PageSize=Kb8]
