``` ini

BenchmarkDotNet=v0.10.14, OS=debian 9
Intel Xeon CPU 2.30GHz, 1 CPU, 1 logical core and 1 physical core
.NET Core SDK=2.1.503
  [Host] : .NET Core 2.1.7 (CoreCLR 4.6.27129.04, CoreFX 4.6.27129.04), 64bit RyuJIT


```
|       Method | PageSize |      Mean |     Error |  Version | Group |
|------------- |--------- |----------:|----------:|--------- |------ |
|       **Search** |      **Kb4** |  **17.21 us** | **0.1504 us** | **0.0.12.0** | **logic** |
|         Scan |      Kb4 | 131.43 us | 1.9137 us | 0.0.12.0 | logic |
|  SearchRange |      Kb4 | 132.04 us | 1.9511 us | 0.0.12.0 | logic |
| ScanForRange |      Kb4 | 191.42 us | 2.5118 us | 0.0.12.0 | logic |
|       **Search** |      **Kb8** |  **32.10 us** | **0.3736 us** | **0.0.12.0** | **logic** |
|         Scan |      Kb8 | 281.17 us | 3.8836 us | 0.0.12.0 | logic |
|  SearchRange |      Kb8 | 245.08 us | 3.3656 us | 0.0.12.0 | logic |
| ScanForRange |      Kb8 | 446.34 us | 6.2968 us | 0.0.12.0 | logic |
