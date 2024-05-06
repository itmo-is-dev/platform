using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Itmo.Dev.Platform.Persistence.Postgres.Tests.Fixtures;
using Itmo.Dev.Platform.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Itmo.Dev.Platform.Persistence.Postgres.Tests;

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

        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPersistenceConnectionProvider>();
        await using var connection = await connectionProvider.GetConnectionAsync(default);

        const string migrateSql = """
        create table test
        (
            values int[] not null 
        )
        """;

        await using (var migrateCommand = connection.CreateCommand(migrateSql))
        {
            await migrateCommand.ExecuteNonQueryAsync(default);
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

        await using (var insertCommand = connection.CreateCommand(insertSql))
        {
            insertCommand.AddMultiArrayStringParameter("values", values);
            await insertCommand.ExecuteNonQueryAsync(default);
        }

        // Cleanup
        const string cleanupSql = """
        drop table test;
        """;

        await using (var cleanupCommand = connection.CreateCommand(cleanupSql))
        {
            await cleanupCommand.ExecuteNonQueryAsync(default);
        }
    }

    public async Task DisposeAsync()
    {
        await _fixture.ResetAsync();
    }
}