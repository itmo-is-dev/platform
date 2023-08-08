using Xunit;

namespace Itmo.Dev.Platform.Postgres.Tests.Fixtures;

[CollectionDefinition(nameof(PostgresCollectionFixture))]
public class PostgresCollectionFixture : ICollectionFixture<PostgresDatabaseFixture> { }