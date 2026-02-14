using Itmo.Dev.Platform.MessagePersistence.Internal.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Buffering;

internal interface IBufferingStepPublisher
{
    Task PublishAsync(IEnumerable<PersistedMessageModel> messages, CancellationToken cancellationToken);
}
