using Itmo.Dev.Platform.MessagePersistence.Internal.Factory;
using Itmo.Dev.Platform.MessagePersistence.Internal.Factory.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Configuration.Migrations;

internal sealed class InitialPayloadMigrationConfigurationLink<TPayload>
    : IPayloadMigrationConfigurationLink<TPayload>
    where TPayload : IPersistedMessagePayload<TPayload>
{
    public IPayloadMigrationLink CreateLink(IServiceProvider serviceProvider)
    {
        return new InitialPayloadMigrationLink<TPayload>(
            serviceProvider.GetRequiredService<IOptions<JsonSerializerSettings>>());
    }
}
