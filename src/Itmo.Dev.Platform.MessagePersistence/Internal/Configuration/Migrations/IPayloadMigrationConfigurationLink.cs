using Itmo.Dev.Platform.MessagePersistence.Internal.Factory;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Configuration.Migrations;

internal interface IPayloadMigrationConfigurationLink<TPayload>
    where TPayload : IPersistedMessagePayload<TPayload>
{
    IPayloadMigrationLink CreateLink(IServiceProvider serviceProvider);
}
