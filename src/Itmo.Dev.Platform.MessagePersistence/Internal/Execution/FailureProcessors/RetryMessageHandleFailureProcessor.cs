using Itmo.Dev.Platform.MessagePersistence.Internal.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Execution.FailureProcessors;

internal class RetryMessageHandleFailureProcessor : IMessageHandleFailureProcessor
{
    public void Process(
        MessageHandleFailureProcessorContext context,
        MessageHandleResult.Failure failure,
        PersistedMessageModel message)
    {
        message.RetryCount++;

        message.State = message.RetryCount < context.HandlerOptions.RetryCount
            ? MessageState.Pending
            : MessageState.Failed;
    }
}
