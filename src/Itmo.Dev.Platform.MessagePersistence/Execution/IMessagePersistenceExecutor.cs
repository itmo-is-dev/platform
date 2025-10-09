using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Execution;

internal interface IMessagePersistenceExecutor
{
    Task ExecuteAsync(
        string messageName,
        MessagePersistenceExecutionConfiguration configuration,
        IEnumerable<SerializedMessage> messages,
        CancellationToken cancellationToken);
}
