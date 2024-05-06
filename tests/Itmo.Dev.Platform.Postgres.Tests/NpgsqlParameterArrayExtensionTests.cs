using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.Tests.Fixtures;
using Itmo.Dev.Platform.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace Itmo.Dev.Platform.Postgres.Tests;

[Collection(nameof(PostgresCollectionFixture))]
public class NpgsqlParameterArrayExtensionTests : IAsyncDisposeLifetime
{
    private readonly PostgresDatabaseFixture _fixture;

    public NpgsqlParameterArrayExtensionTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddMultiArrayStringParameter_ShouldInsertValuesCorrectly()
    {
        // Arrange
        await using var scope = _fixture.Scope;

        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPostgresConnectionProvider>();
        await using var connection = await connectionProvider.GetConnectionAsync(default);

        const string migrateSql = """
        create table test
        (
            values int[] not null 
        )
        """;

        await using (var migrateCommand = new NpgsqlCommand(migrateSql, connection))
        {
            await migrateCommand.ExecuteNonQueryAsync();
        }

        int[][] values =
        {
            new[] { 1, 2, 3 },
            new[] { 4, 5, 6 },
        };

        // Act
        const string insertSql = """
        insert into test(values)
        select s.value::int[] from unnest(:values) as s(value);
        """;

        await using (var insertCommand = new NpgsqlCommand(insertSql, connection))
        {
            insertCommand.AddMultiArrayStringParameter("values", values);
            await insertCommand.ExecuteNonQueryAsync();
        }

        // Cleanup
        const string cleanupSql = """
        drop table test;
        """;

        await using (var cleanupCommand = new NpgsqlCommand(cleanupSql, connection))
        {
            await cleanupCommand.ExecuteNonQueryAsync();
        }
    }

    public async Task DisposeAsync()
    {
        await _fixture.ResetAsync();
    }
}