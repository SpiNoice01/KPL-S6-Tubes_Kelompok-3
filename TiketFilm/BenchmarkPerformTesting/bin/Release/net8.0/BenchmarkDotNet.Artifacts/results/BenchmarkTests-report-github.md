```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
12th Gen Intel Core i5-12500H, 1 CPU, 16 logical and 12 physical cores
.NET SDK 9.0.201
  [Host]     : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2 [AttachedDebugger]
  DefaultJob : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2


```
| Method                          | Mean         | Error        | StdDev       | Gen0      | Gen1      | Gen2     | Allocated   |
|-------------------------------- |-------------:|-------------:|-------------:|----------:|----------:|---------:|------------:|
| BenchmarkGetMovies              |     60.11 μs |     1.582 μs |     4.590 μs |    1.2207 |         - |        - |    12.18 KB |
| BenchmarkOrderTicket            | 23,945.43 μs | 1,954.686 μs | 5,763.437 μs | 3468.7500 | 3468.7500 | 984.3750 | 18991.38 KB |
| BenchmarkShowTransactionHistory |  1,863.68 μs |    97.537 μs |   287.591 μs |         - |         - |        - |    22.49 KB |
