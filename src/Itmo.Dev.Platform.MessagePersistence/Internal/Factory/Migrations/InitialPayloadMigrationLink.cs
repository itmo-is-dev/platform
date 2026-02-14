using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Factory.Migrations;

internal sealed class InitialPayloadMigrationLink<TPayload>(
    IOptions<JsonSerializerSettings> serializerSettings)
    : IPayloadMigrationLink
    where TPayload : IPersistedMessagePayload<TPayload>
{
    public ValueTask<IPersistedMessagePayload> MigrateAsync(
        PersistedMessageModel message,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        if (message.Version != PayloadVersion.Default)
            throw MessagePersistenceException.CouldNotFindMigrator(message.Name, message.Version);

        var deserialized = JsonConvert.DeserializeObject<TPayload>(message.Value, serializerSettings.Value);

        if (deserialized is not IPersistedMessagePayload payload)
        {
            throw MessagePersistenceException.FailedToDeserializePayload(
                message.Name,
                message.Version,
                deserialized);
        }

        return ValueTask.FromResult(payload);
    }
}
