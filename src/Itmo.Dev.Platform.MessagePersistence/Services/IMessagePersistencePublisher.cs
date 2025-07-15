using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Services;

internal interface IMessagePersistencePublisher
{
    Task PublishAsync(SerializedMessage[] messages, CancellationToken cancellationToken);
}
