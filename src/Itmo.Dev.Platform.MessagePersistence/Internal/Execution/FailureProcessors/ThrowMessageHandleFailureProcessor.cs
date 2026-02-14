using Itmo.Dev.Platform.MessagePersistence.Internal.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Execution.FailureProcessors;

internal class ThrowMessageHandleFailureProcessor : IMessageHandleFailureProcessor
{
    public void Process(
        MessageHandleFailureProcessorContext context,
        MessageHandleResult.Failure failure,
        PersistedMessageModel message)
    {
        Exception exception =
            failure.Exception ?? MessagePersistenceException.MessageHandleFailed(message.Name, message.Id);

        context.AddException(exception);
    }
}
