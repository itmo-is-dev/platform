using FluentAssertions;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.Tests.Fixtures;
using Itmo.Dev.Platform.Postgres.UnitOfWork;
using Itmo.Dev.Platform.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;
using Xunit;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.Postgres.Tests;

[Collection(nameof(PostgresCollectionFixture))]
public class UnitOfWorkTests : TestBase, IAsyncDisposeLifetime
{
    private readonly PostgresDatabaseFixture _fixture;

    public UnitOfWorkTests(PostgresDatabaseFixture fixture, ITestOutputHelper output) : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CommitAsync_ShouldAddValues_WhenAddCommandSupplied()
    {
        // Arrange
        const long id = 12;
        const string value = "aboba";

        await using var scope = _fixture.Scope;

        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPostgresConnectionProvider>();
        var connection = await connectionProvider.GetConnectionAsync(default);

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        const string createSql = """
        create table test
        (
            id bigint not null ,
            value text not null 
        );
        """;

        await using var createCommand = new NpgsqlCommand(createSql, connection);
        await createCommand.ExecuteNonQueryAsync();

        // Act 
        const string insertSql = """
        insert into test (id, value)
        values (:id, :value);
        """;

        var insertCommand = new NpgsqlCommand(insertSql)
            .AddParameter("id", id)
            .AddParameter("value", value);

        unitOfWork.Enqueue(insertCommand);

        await unitOfWork.CommitAsync(IsolationLevel.ReadCommitted, default);

        // Assert
        const string querySql = """
        select id, value from test
        """;

        await using (var queryCommand = new NpgsqlCommand(querySql, connection))
        {
            await using var reader = await queryCommand.ExecuteReaderAsync();

            var hasNext = await reader.ReadAsync();
            hasNext.Should().BeTrue();

            reader.GetInt64(0).Should().Be(id);
            reader.GetString(1).Should().Be(value);

            hasNext = await reader.ReadAsync();
            hasNext.Should().BeFalse();
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

    [Fact]
    public async Task CommitAsync_ShouldEmptyUnitOfWork()
    {
        // Arrange
        const string migrateSql = """
        create table test(id bigint);
        """;

        await using var scope = _fixture.Scope;

        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPostgresConnectionProvider>();
        var connection = await connectionProvider.GetConnectionAsync(default);

        var unitOfWork = (ReusableUnitOfWork)scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await using (var migrateCommand = new NpgsqlCommand(migrateSql, connection))
        {
            await migrateCommand.ExecuteNonQueryAsync();
        }

        // Act
        const string actSql = """
        insert into test(id) values(:id);
        """;

        await using var actCommand = new NpgsqlCommand(actSql).AddParameter("id", 1);
        unitOfWork.Enqueue(actCommand);

        await unitOfWork.CommitAsync(IsolationLevel.ReadCommitted, default);

        // Assert
        unitOfWork.Count.Should().Be(0);
    }

    public async Task DisposeAsync()
    {
        await _fixture.ResetAsync();
    }
}