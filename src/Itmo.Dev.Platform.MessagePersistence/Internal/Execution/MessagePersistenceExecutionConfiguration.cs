using Itmo.Dev.Platform.MessagePersistence.Internal.Execution.FailureProcessors;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Execution;

internal record MessagePersistenceExecutionConfiguration(
    IMessageHandleFailureProcessor FailureProcessor);
