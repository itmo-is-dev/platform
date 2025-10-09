using Itmo.Dev.Platform.MessagePersistence.Exceptions;
using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Execution.FailureProcessors;

internal class ThrowMessageHandleFailureProcessor : IMessageHandleFailureProcessor
{
    public void Process(
        MessageHandleFailureProcessorContext context,
        MessageHandleResult.Failure failure,
        SerializedMessage message)
    {
        Exception exception =
            failure.Exception ?? MessagePersistenceException.MessageHandleFailed(message.Name, message.Id);

        context.AddException(exception);
    }
}
