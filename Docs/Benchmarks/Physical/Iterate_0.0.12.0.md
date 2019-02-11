``` ini

BenchmarkDotNet=v0.10.14, OS=debian 9
Intel Xeon CPU 2.30GHz, 1 CPU, 1 logical core and 1 physical core
.NET Core SDK=2.1.503
  [Host] : .NET Core 2.1.7 (CoreCLR 4.6.27129.04, CoreFX 4.6.27129.04), 64bit RyuJIT


```
|      Method | WriteMethod |     Mean |    Error |  Version |   Group |
|------------ |------------ |---------:|---------:|--------- |-------- |
| IteratePage |   FixedSize | 578.6 us | 8.179 us | 0.0.12.0 | Iterate |
