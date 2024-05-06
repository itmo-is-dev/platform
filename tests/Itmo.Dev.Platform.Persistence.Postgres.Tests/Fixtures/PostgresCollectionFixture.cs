using Xunit;

namespace Itmo.Dev.Platform.Persistence.Postgres.Tests.Fixtures;

[CollectionDefinition(nameof(PostgresCollectionFixture))]
public class PostgresCollectionFixture : ICollectionFixture<PostgresDatabaseFixture> { }