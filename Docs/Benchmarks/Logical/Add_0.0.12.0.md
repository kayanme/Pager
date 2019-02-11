``` ini

BenchmarkDotNet=v0.10.14, OS=debian 9
Intel Xeon CPU 2.30GHz, 1 CPU, 1 logical core and 1 physical core
.NET Core SDK=2.1.503
  [Host] : .NET Core 2.1.7 (CoreCLR 4.6.27129.04, CoreFX 4.6.27129.04), 64bit RyuJIT


```
|                    Method | PageSize |         Mean |      Error |  Version | Group |
|-------------------------- |--------- |-------------:|-----------:|--------- |------ |
|        **AddRecordWithOrder** |      **Kb4** |     **3.006 us** |  **0.0386 us** | **0.0.12.0** | **logic** |
|    AddRecordInVirtualPage |      Kb4 |    26.436 us |  2.3768 us | 0.0.12.0 | logic |
|     AddRecordWithoutOrder |      Kb4 |     1.323 us |  0.0252 us | 0.0.12.0 | logic |
|     ChangeRecordWithOrder |      Kb4 |   509.527 us |  5.3732 us | 0.0.12.0 | logic |
| ChangeRecordInVirtualPage |      Kb4 |    10.385 us |  0.1068 us | 0.0.12.0 | logic |
|  ChangeRecordWithoutOrder |      Kb4 |     2.444 us |  0.0770 us | 0.0.12.0 | logic |
|        **AddRecordWithOrder** |      **Kb8** |     **4.920 us** |  **0.0593 us** | **0.0.12.0** | **logic** |
|    AddRecordInVirtualPage |      Kb8 |    34.815 us |  2.3578 us | 0.0.12.0 | logic |
|     AddRecordWithoutOrder |      Kb8 |     1.533 us |  0.0379 us | 0.0.12.0 | logic |
|     ChangeRecordWithOrder |      Kb8 | 1,044.136 us | 21.0318 us | 0.0.12.0 | logic |
| ChangeRecordInVirtualPage |      Kb8 |    36.582 us |  0.4570 us | 0.0.12.0 | logic |
|  ChangeRecordWithoutOrder |      Kb8 |     6.317 us |  0.2573 us | 0.0.12.0 | logic |
