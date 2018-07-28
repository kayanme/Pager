``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i7-4510U CPU 2.00GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
Frequency=2533201 Hz, Resolution=394.7575 ns, Timer=TSC
.NET Core SDK=2.1.202
  [Host] : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT

AnalyzeLaunchVariance=True  Runtime=Core  InvocationCount=192  
LaunchCount=3  

```
|                  Method |     Mean |     Error |    StdDev |   Median |
|------------------------ |---------:|----------:|----------:|---------:|
|    NaiveReadTakeRelease | 172.3 ns |  3.334 ns |  6.581 ns | 171.8 ns |
| NaiveTwoReadTakeRelease | 354.7 ns |  8.311 ns | 41.125 ns | 342.5 ns |
|     PageReadTakeRelease | 517.2 ns | 19.202 ns | 61.792 ns | 488.7 ns |
|  PageTwoReadTakeRelease | 915.3 ns | 15.132 ns | 71.995 ns | 884.7 ns |
|   NaiveWriteTakeRelease | 134.0 ns |  3.535 ns | 11.015 ns | 131.6 ns |
|    PageWriteTakeRelease | 494.2 ns | 12.203 ns | 25.473 ns | 486.0 ns |
