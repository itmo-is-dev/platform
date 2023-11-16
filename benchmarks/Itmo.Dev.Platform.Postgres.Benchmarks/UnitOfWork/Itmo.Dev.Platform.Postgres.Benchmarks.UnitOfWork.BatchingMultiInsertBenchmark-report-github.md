```

BenchmarkDotNet v0.13.10, macOS Sonoma 14.1 (23B74) [Darwin 23.1.0]
Apple M1 Pro, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.100-rc.2.23502.2
  [Host]     : .NET 7.0.13 (7.0.1323.51816), Arm64 RyuJIT AdvSIMD
  Job-JLECWE : .NET 7.0.13 (7.0.1323.51816), Arm64 RyuJIT AdvSIMD

InvocationCount=1  UnrollFactor=1  

```

| Method   | Size  |         Mean |      Error |      StdDev |      Gen0 |      Gen1 |   Allocated |
|----------|-------|-------------:|-----------:|------------:|----------:|----------:|------------:|
| Arrays   | 10    |     1.118 ms |  0.0474 ms |   0.1329 ms |         - |         - |     4.94 KB |
| Sequence | 10    |     5.097 ms |  0.1165 ms |   0.3361 ms |         - |         - |    26.12 KB |
| Batch    | 10    |     1.135 ms |  0.0439 ms |   0.1196 ms |         - |         - |    16.59 KB |
| Arrays   | 100   |     1.292 ms |  0.0705 ms |   0.1941 ms |         - |         - |    12.09 KB |
| Sequence | 100   |    41.823 ms |  0.8313 ms |   1.8592 ms |         - |         - |   220.88 KB |
| Batch    | 100   |     2.595 ms |  0.0543 ms |   0.1566 ms |         - |         - |   141.37 KB |
| Arrays   | 1000  |     2.698 ms |  0.0591 ms |   0.1619 ms |         - |         - |     71.9 KB |
| Sequence | 1000  |   410.890 ms |  7.9593 ms |  12.3917 ms |         - |         - |  2196.66 KB |
| Batch    | 1000  |    14.577 ms |  0.2557 ms |   0.3748 ms |         - |         - |  1383.29 KB |
| Arrays   | 10000 |    14.597 ms |  0.2828 ms |   0.3026 ms |         - |         - |   249.65 KB |
| Sequence | 10000 | 4,391.269 ms | 84.2895 ms | 145.3953 ms | 3000.0000 |         - | 21954.48 KB |
| Batch    | 10000 |   128.540 ms |  2.5681 ms |   4.4978 ms | 2000.0000 | 1000.0000 | 13300.05 KB |
