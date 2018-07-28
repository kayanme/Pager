``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i7-4510U CPU 2.00GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
Frequency=2533201 Hz, Resolution=394.7575 ns, Timer=TSC
.NET Core SDK=2.1.202
  [Host] : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT

AnalyzeLaunchVariance=True  Runtime=Core  InvocationCount=192  
LaunchCount=3  

```
|       Method |        WriteMethod | PageSize |      Mean |    Error |    StdDev |    Median |
|------------- |------------------- |--------- |----------:|---------:|----------:|----------:|
|   **ScanSearch** |          **FixedSize** |      **Kb4** |  **75.14 us** | **3.645 us** | **18.473 us** |  **66.66 us** |
| BinarySearch |          FixedSize |      Kb4 |  21.50 us | 1.921 us |  9.825 us |  19.66 us |
|   **ScanSearch** |          **FixedSize** |      **Kb8** | **137.02 us** | **3.572 us** | **18.263 us** | **132.71 us** |
| BinarySearch |          FixedSize |      Kb8 |  37.80 us | 1.899 us |  9.308 us |  37.57 us |
|   **ScanSearch** | **FixedSizeWithOrder** |      **Kb4** |  **69.14 us** | **1.066 us** |  **5.395 us** |  **68.42 us** |
| BinarySearch | FixedSizeWithOrder |      Kb4 |  24.71 us | 3.173 us | 13.402 us |  19.91 us |
|   **ScanSearch** | **FixedSizeWithOrder** |      **Kb8** | **135.27 us** | **1.991 us** |  **8.363 us** | **134.34 us** |
| BinarySearch | FixedSizeWithOrder |      Kb8 |  46.78 us | 4.639 us | 23.763 us |  44.56 us |
