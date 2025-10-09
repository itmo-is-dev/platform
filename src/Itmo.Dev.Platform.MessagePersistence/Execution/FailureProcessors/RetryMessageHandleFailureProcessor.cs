using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Execution.FailureProcessors;

internal class RetryMessageHandleFailureProcessor : IMessageHandleFailureProcessor
{
    public void Process(
        MessageHandleFailureProcessorContext context,
        MessageHandleResult.Failure failure,
        SerializedMessage message)
    {
        message.RetryCount++;

        message.State = message.RetryCount < context.HandlerOptions.RetryCount
            ? MessageState.Pending
            : MessageState.Failed;
    }
}
