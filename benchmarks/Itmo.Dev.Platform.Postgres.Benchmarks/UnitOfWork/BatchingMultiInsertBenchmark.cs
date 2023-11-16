using BenchmarkDotNet.Attributes;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.Tests.Fixtures;
using Npgsql;

namespace Itmo.Dev.Platform.Postgres.Benchmarks.UnitOfWork;

[MemoryDiagnoser(true)]
public class BatchingMultiInsertBenchmark
{
    private PostgresDatabaseFixture _fixture = null!;
    private NpgsqlConnection _connection = null!;

    private IReadOnlyCollection<CreateUserModel> _models = null!;

    [Params(10, 100, 1000, 10_000)]
    public int Size { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _fixture = new PostgresDatabaseFixture();
        _fixture.InitializeAsync().GetAwaiter().GetResult();

        _connection = _fixture.Connection;
        _connection.Open();

        const string migrationSql = """
        create table users
        (
            id bigint primary key generated always as identity ,
            name text not null ,
            age int not null 
        );
        """;

        using (var migrationCommand = new NpgsqlCommand(migrationSql, _connection))
        {
            migrationCommand.ExecuteNonQueryAsync().GetAwaiter().GetResult();
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _fixture.DisposeAsync().GetAwaiter().GetResult();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _models = Enumerable
            .Range(0, Size)
            .Select(x => new CreateUserModel(x.ToString(), x))
            .ToArray();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        const string cleanupSql = """
        truncate table users;
        """;

        using (var cleanupCommand = new NpgsqlCommand(cleanupSql, _connection))
        {
            cleanupCommand.ExecuteNonQueryAsync().GetAwaiter().GetResult();
        }
    }

    [Benchmark]
    public async Task Arrays()
    {
        const string sql = """
        insert into users(name, age)
        select * from unnest(:names, :ages);
        """;

        await using var command = new NpgsqlCommand(sql, _connection)
            .AddParameter("names", _models.Select(x => x.Name).ToArray())
            .AddParameter("ages", _models.Select(x => x.Age).ToArray());

        await command.ExecuteNonQueryAsync();
    }

    [Benchmark]
    public async Task Sequence()
    {
        const string sql = """
        insert into users(name, age)
        values (:name, :age);
        """;

        foreach (CreateUserModel model in _models)
        {
            await using var command = new NpgsqlCommand(sql, _connection)
                .AddParameter("name", model.Name)
                .AddParameter("age", model.Age);

            await command.ExecuteNonQueryAsync();
        }
    }

    [Benchmark]
    public async Task Batch()
    {
        const string sql = """
        insert into users(name, age)
        values (:name, :age);
        """;

        await using var batch = new NpgsqlBatch(_connection);

        foreach (CreateUserModel model in _models)
        {
            await using var command = new NpgsqlCommand(sql)
                .AddParameter("name", model.Name)
                .AddParameter("age", model.Age);

            batch.BatchCommands.Add(command.ToBatchCommand());
        }

        await batch.ExecuteNonQueryAsync();
    }

    public sealed record CreateUserModel(string Name, int Age);
}