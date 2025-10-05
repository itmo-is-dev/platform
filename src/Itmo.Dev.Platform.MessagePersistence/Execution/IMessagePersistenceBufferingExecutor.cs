using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Execution;

internal interface IMessagePersistenceBufferingExecutor
{
    Task ExecuteAsync(
        string messageName,
        string bufferingStepName,
        IEnumerable<SerializedMessage> messages,
        CancellationToken cancellationToken);
}
