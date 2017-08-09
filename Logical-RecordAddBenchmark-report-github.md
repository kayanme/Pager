``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 8.1 (6.3.9600)
Processor=Intel Core i7-4510U CPU 2.00GHz (Haswell), ProcessorCount=4
Frequency=2533191 Hz, Resolution=394.7590 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0
  Job-YKXKBQ : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0

AnalyzeLaunchVariance=True  Runtime=Clr  InvocationCount=96  
LaunchCount=3  

```
 |                    Method | PageSize |           Mean |        Error |       StdDev |         Median |
 |-------------------------- |--------- |---------------:|-------------:|-------------:|---------------:|
 |        **AddRecordWithOrder** |      **Kb4** |       **313.1 ns** |     **19.34 ns** |     **81.23 ns** |       **280.6 ns** |
 |    AddRecordInVirtualPage |      Kb4 |     3,168.9 ns |    189.70 ns |    933.31 ns |     2,664.2 ns |
 |     AddRecordWithoutOrder |      Kb4 |       294.9 ns |     15.70 ns |     79.26 ns |       262.6 ns |
 |     ChangeRecordWithOrder |      Kb4 |   560,983.3 ns |  7,585.27 ns | 35,420.78 ns |   550,498.4 ns |
 | ChangeRecordInVirtualPage |      Kb4 |     3,066.9 ns |    162.22 ns |    796.61 ns |     2,675.8 ns |
 |  ChangeRecordWithoutOrder |      Kb4 |     1,561.7 ns |     94.57 ns |    412.60 ns |     1,307.6 ns |
 |        **AddRecordWithOrder** |      **Kb8** |       **346.1 ns** |     **19.39 ns** |     **97.39 ns** |       **294.7 ns** |
 |    AddRecordInVirtualPage |      Kb8 |     2,696.7 ns |    127.88 ns |    624.37 ns |     2,450.6 ns |
 |     AddRecordWithoutOrder |      Kb8 |       310.9 ns |     18.39 ns |     89.81 ns |       268.5 ns |
 |     ChangeRecordWithOrder |      Kb8 | 1,064,584.3 ns | 16,153.83 ns | 41,698.19 ns | 1,052,515.0 ns |
 | ChangeRecordInVirtualPage |      Kb8 |     2,745.1 ns |     78.69 ns |    384.93 ns |     2,655.8 ns |
 |  ChangeRecordWithoutOrder |      Kb8 |     1,451.4 ns |     75.29 ns |    330.08 ns |     1,304.9 ns |
