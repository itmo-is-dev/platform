using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Execution.FailureProcessors;

internal interface IMessageHandleFailureProcessor
{
    void Process(
        MessageHandleFailureProcessorContext context,
        MessageHandleResult.Failure failure,
        SerializedMessage message);
}
