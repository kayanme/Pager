``` ini

BenchmarkDotNet=v0.10.14, OS=debian 9
Intel Xeon CPU 2.30GHz, 1 CPU, 1 logical core and 1 physical core
.NET Core SDK=2.1.503
  [Host] : .NET Core 2.1.7 (CoreCLR 4.6.27129.04, CoreFX 4.6.27129.04), 64bit RyuJIT


```
|       Method |        WriteMethod | PageSize |      Mean |     Error |  Version |  Group |
|------------- |------------------- |--------- |----------:|----------:|--------- |------- |
|   **ScanSearch** |          **FixedSize** |      **Kb4** |  **92.03 us** | **1.2131 us** | **0.0.12.0** | **Search** |
| BinarySearch |          FixedSize |      Kb4 |  12.29 us | 0.0935 us | 0.0.12.0 | Search |
|   **ScanSearch** |          **FixedSize** |      **Kb8** | **208.24 us** | **2.4788 us** | **0.0.12.0** | **Search** |
| BinarySearch |          FixedSize |      Kb8 |  19.05 us | 0.1540 us | 0.0.12.0 | Search |
|   **ScanSearch** | **FixedSizeWithOrder** |      **Kb4** |  **99.40 us** | **1.1981 us** | **0.0.12.0** | **Search** |
| BinarySearch | FixedSizeWithOrder |      Kb4 |  14.90 us | 0.1673 us | 0.0.12.0 | Search |
|   **ScanSearch** | **FixedSizeWithOrder** |      **Kb8** | **234.45 us** | **2.7654 us** | **0.0.12.0** | **Search** |
| BinarySearch | FixedSizeWithOrder |      Kb8 |  24.42 us | 0.2258 us | 0.0.12.0 | Search |
