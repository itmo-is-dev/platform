using FluentAssertions;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.Tests.Fixtures;
using Itmo.Dev.Platform.Postgres.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;
using Xunit;

namespace Itmo.Dev.Platform.Postgres.Tests;

[Collection(nameof(PostgresCollectionFixture))]
public class UnitOfWorkTests
{
    private readonly PostgresDatabaseFixture _fixture;

    public UnitOfWorkTests(PostgresDatabaseFixture fixture)
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

        await using var queryCommand = new NpgsqlCommand(querySql, connection);
        await using var reader = await queryCommand.ExecuteReaderAsync();

        var hasNext = await reader.ReadAsync();
        hasNext.Should().BeTrue();

        reader.GetInt64(0).Should().Be(id);
        reader.GetString(1).Should().Be(value);

        hasNext = await reader.ReadAsync();
        hasNext.Should().BeFalse();
    }
}