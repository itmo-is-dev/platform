using Itmo.Dev.Platform.MessagePersistence.Internal.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Execution;

internal interface IMessagePersistenceExecutor
{
    Task ExecuteAsync(
        string messageName,
        MessagePersistenceExecutionConfiguration configuration,
        IEnumerable<PersistedMessageModel> messages,
        CancellationToken cancellationToken);
}
