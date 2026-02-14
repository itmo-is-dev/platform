using Itmo.Dev.Platform.MessagePersistence.Internal.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Execution.FailureProcessors;

internal interface IMessageHandleFailureProcessor
{
    void Process(
        MessageHandleFailureProcessorContext context,
        MessageHandleResult.Failure failure,
        PersistedMessageModel message);
}
