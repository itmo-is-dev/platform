using Itmo.Dev.Platform.MessagePersistence.Internal.Metrics;
using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Factory.Migrations;

internal sealed class VersionedPayloadMigrationLink<TPayload, TPrevious>(
    IPayloadMigrationLink previousMigration,
    PayloadVersion version,
    IOptions<JsonSerializerSettings> serializerSettings,
    IMessagePersistenceMetrics metrics)
    : IPayloadMigrationLink
    where TPrevious : IPersistedMessagePayload<TPrevious>
    where TPayload : IPersistedMessagePayload<TPayload, TPrevious>
{
    public async ValueTask<IPersistedMessagePayload> MigrateAsync(
        PersistedMessageModel message,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        if (message.Version == version)
        {
            var deserialized = JsonConvert.DeserializeObject<TPayload>(message.Value, serializerSettings.Value);

            if (deserialized is not IPersistedMessagePayload payload)
            {
                throw MessagePersistenceException.FailedToDeserializePayload(
                    message.Name,
                    message.Version,
                    deserialized);
            }

            return payload;
        }

        var result = await previousMigration.MigrateAsync(message, serviceProvider, cancellationToken);

        if (result is not TPrevious previous)
        {
            throw MessagePersistenceException.InvalidPreviousPayload(
                message.Name,
                message.Version,
                version,
                typeof(TPayload),
                result.GetType());
        }

        var current = await TPayload.MigrateAsync(previous, serviceProvider, cancellationToken);
        metrics.IncMessagePayloadMigrated(message.Name, message.Version, version);

        message.Version = version;
        message.Key = JsonConvert.SerializeObject(current.Key, serializerSettings.Value);
        message.Value = JsonConvert.SerializeObject(current, serializerSettings.Value);

        return current;
    }
}
