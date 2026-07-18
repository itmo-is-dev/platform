using FluentAssertions;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Itmo.Dev.Platform.Persistence.Postgres.Tests.Fixtures;
using Itmo.Dev.Platform.Persistence.Postgres.Tests.Models;
using Itmo.Dev.Platform.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Itmo.Dev.Platform.Persistence.Postgres.Tests;

[Collection(nameof(PostgresCollectionFixture))]
public sealed class PostgresPersistenceCommandTests(PostgresDatabaseFixture fixture) : IAsyncDisposeLifetime
{
    public Task DisposeAsync() => fixture.ResetAsync();

    [Fact]
    public async Task Test()
    {
        // Arrange
        await using var scope = fixture.Scope;
        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPersistenceConnectionProvider>();

        await using var connection = await connectionProvider.GetConnectionAsync(default);

        var id = new LongId(1);
        object[] collection = [id];

        // Act
        await using var command = connection
            .CreateCommand("SELECT :values")
            .AddParameter("values", collection, el => (LongId?)el);

        await using var reader = await command.ExecuteReaderAsync(default);
        await reader.ReadAsync();

        // Assert
        reader.GetFieldValue<LongId[]>(0).Should().ContainSingle().Which.Should().Be(id);
    }
}
