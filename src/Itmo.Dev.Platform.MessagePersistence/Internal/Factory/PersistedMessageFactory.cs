using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Itmo.Dev.Platform.MessagePersistence.Internal.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Factory;

internal sealed class PersistedMessageFactory(
    IServiceProvider serviceProvider,
    MessagePersistenceRegistry registry)
    : IPersistedMessageFactory
{
    public async ValueTask<IPersistedMessage> CreateAsync(
        PersistedMessageModel message,
        CancellationToken cancellationToken)
    {
        var migration = serviceProvider.GetRequiredKeyedService<IPayloadMigrationLink>(message.Name);
        var payload = await migration.MigrateAsync(message, serviceProvider, cancellationToken);

        var messageRecord = registry.GetRecord(message.Name);
        return messageRecord.Factory(payload);
    }
}
