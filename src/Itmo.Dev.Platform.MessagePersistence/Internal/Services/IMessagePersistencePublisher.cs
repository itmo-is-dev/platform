using Itmo.Dev.Platform.MessagePersistence.Internal.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Services;

internal interface IMessagePersistencePublisher
{
    Task PublishAsync(PersistedMessageModel[] messages, CancellationToken cancellationToken);
}
