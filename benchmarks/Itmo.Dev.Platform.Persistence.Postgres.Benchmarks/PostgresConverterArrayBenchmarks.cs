using BenchmarkDotNet.Attributes;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Itmo.Dev.Platform.Persistence.Postgres.Tests.Fixtures;
using Itmo.Dev.Platform.Persistence.Postgres.Tests.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Persistence.Postgres.Benchmarks;

[MemoryDiagnoser]
public class PostgresConverterArrayBenchmarks
{
    private readonly PostgresDatabaseFixture _fixture = new();
    private IPersistenceConnection _connection = null!;

    private LongId[] _wrappedIds = [];
    private long[] _unwrappedIds = [];

    [Params(10, 100, 1000)]
    public int Count { get; set; }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        await _fixture.InitializeAsync();

        _connection = await _fixture.Scope.ServiceProvider
            .GetRequiredService<IPersistenceConnectionProvider>()
            .GetConnectionAsync(default);

        _wrappedIds = Enumerable.Range(1, Count).Select(x => new LongId(x)).ToArray();
        _unwrappedIds = Enumerable.Range(1, Count).Select(long (x) => x).ToArray();
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await _fixture.DisposeAsync();
    }

    [Benchmark(Baseline = true)]
    public async Task<int> Unwrapped()
    {
        const string sql = "SELECT cardinality(:values);";

        await using var command = _connection.CreateCommand(sql)
            .AddParameter("values", _unwrappedIds);

        await using var reader = await command.ExecuteReaderAsync(default);
        await reader.ReadAsync();

        return reader.GetInt32(0);
    }

    [Benchmark]
    public async Task<int> UnwrapByHand()
    {
        const string sql = "SELECT cardinality(:values);";

        await using var command = _connection.CreateCommand(sql)
            .AddParameter("values", _wrappedIds.Select(x => x.Value));

        await using var reader = await command.ExecuteReaderAsync(default);
        await reader.ReadAsync();

        return reader.GetInt32(0);
    }

    [Benchmark]
    public async Task<int> UnwrapByLazyCollection()
    {
        const string sql = "SELECT cardinality(:values);";

        await using var command = _connection.CreateCommand(sql)
            .AddParameter("values", _wrappedIds, x => x.Value);

        await using var reader = await command.ExecuteReaderAsync(default);
        await reader.ReadAsync();

        return reader.GetInt32(0);
    }

    [Benchmark]
    public async Task<int> UnwrapByConversion()
    {
        const string sql = "SELECT cardinality(:values);";

        await using var command = _connection.CreateCommand(sql)
            .AddParameter("values", _wrappedIds);

        await using var reader = await command.ExecuteReaderAsync(default);
        await reader.ReadAsync();

        return reader.GetInt32(0);
    }
}
