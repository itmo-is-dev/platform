using Itmo.Dev.Platform.MessagePersistence.Execution.FailureProcessors;

namespace Itmo.Dev.Platform.MessagePersistence.Execution;

internal record MessagePersistenceExecutionConfiguration(
    IMessageHandleFailureProcessor FailureProcessor);
