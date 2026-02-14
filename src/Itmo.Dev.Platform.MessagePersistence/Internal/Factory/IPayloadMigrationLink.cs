using Itmo.Dev.Platform.MessagePersistence.Internal.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Factory;

internal interface IPayloadMigrationLink
{
    ValueTask<IPersistedMessagePayload> MigrateAsync(
        PersistedMessageModel message,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}
