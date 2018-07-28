``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i7-4510U CPU 2.00GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
Frequency=2533201 Hz, Resolution=394.7575 ns, Timer=TSC
.NET Core SDK=2.1.202
  [Host] : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT

AnalyzeLaunchVariance=True  Runtime=Core  InvocationCount=192  
LaunchCount=3  

```
|      Method | WriteMethod |     Mean |    Error |   StdDev |
|------------ |------------ |---------:|---------:|---------:|
| IteratePage |   FixedSize | 414.7 us | 4.890 us | 11.72 us |
