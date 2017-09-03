``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 8.1 (6.3.9600)
Processor=Intel Core i7-4510U CPU 2.00GHz (Haswell), ProcessorCount=4
Frequency=2533195 Hz, Resolution=394.7584 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0
  Job-NQIHYO : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2053.0

AnalyzeLaunchVariance=True  Runtime=Clr  InvocationCount=192  
LaunchCount=3  

```
 |                  Method |     Mean |     Error |    StdDev |   Median |
 |------------------------ |---------:|----------:|----------:|---------:|
 |    NaiveReadTakeRelease | 156.0 ns | 10.000 ns |  52.12 ns | 177.7 ns |
 | NaiveTwoReadTakeRelease | 209.0 ns |  7.107 ns |  29.70 ns | 195.2 ns |
 |     PageReadTakeRelease | 258.2 ns | 16.079 ns |  69.98 ns | 262.6 ns |
 |  PageTwoReadTakeRelease | 468.8 ns | 27.691 ns | 125.98 ns | 382.5 ns |
 |   NaiveWriteTakeRelease | 128.8 ns |  7.215 ns |  36.43 ns | 139.0 ns |
 |    PageWriteTakeRelease | 224.6 ns | 11.860 ns |  51.37 ns | 192.0 ns |
