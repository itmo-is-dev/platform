// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Itmo.Dev.Platform.Postgres.Benchmarks.UnitOfWork;

BenchmarkRunner.Run<BatchingMultiInsertBenchmark>(DefaultConfig.Instance.StopOnFirstError());