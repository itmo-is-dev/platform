using Itmo.Dev.Platform.MessagePersistence.Internal.Factory;
using Itmo.Dev.Platform.MessagePersistence.Internal.Factory.Migrations;
using Itmo.Dev.Platform.MessagePersistence.Internal.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Configuration.Migrations;

internal sealed class VersionedPayloadMigrationConfigurationLink<TPayload, TPrevious>(
    IPayloadMigrationConfigurationLink<TPrevious> previous,
    PayloadVersion version
)
    : IPayloadMigrationConfigurationLink<TPayload>
    where TPrevious : IPersistedMessagePayload<TPrevious>
    where TPayload : IPersistedMessagePayload<TPayload, TPrevious>
{
    public IPayloadMigrationLink CreateLink(IServiceProvider serviceProvider)
    {
        return new VersionedPayloadMigrationLink<TPayload, TPrevious>(
            previous.CreateLink(serviceProvider),
            version,
            serviceProvider.GetRequiredService<IOptions<JsonSerializerSettings>>(),
            serviceProvider.GetRequiredService<IMessagePersistenceMetrics>());
    }
}
