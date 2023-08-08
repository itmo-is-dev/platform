using Itmo.Dev.Platform.Testing.Fixtures;
using Itmo.Dev.Platform.Testing.Mocks;

namespace Itmo.Dev.Platform.Testing.Extensions;

public static class DatabaseFixtureExtensions
{
    public static PostgresConnectionProviderMock CreateConnectionProvider(this DatabaseFixture fixture)
    {
        return new PostgresConnectionProviderMock(fixture.Connection);
    }
}