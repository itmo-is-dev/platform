using Itmo.Dev.Platform.MessagePersistence.Internal.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Execution;

internal interface IMessagePersistenceBufferingExecutor
{
    Task ExecuteAsync(
        string messageName,
        string bufferingStepName,
        IEnumerable<PersistedMessageModel> messages,
        CancellationToken cancellationToken);
}
