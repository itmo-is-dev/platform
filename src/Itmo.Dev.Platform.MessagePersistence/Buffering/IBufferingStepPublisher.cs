using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Buffering;

internal interface IBufferingStepPublisher
{
    Task PublishAsync(IEnumerable<SerializedMessage> messages, CancellationToken cancellationToken);
}
