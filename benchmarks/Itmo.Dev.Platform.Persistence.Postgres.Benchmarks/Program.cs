using BenchmarkDotNet.Running;
using Itmo.Dev.Platform.Persistence.Postgres.Benchmarks;

BenchmarkRunner.Run<PostgresConverterArrayBenchmarks>();