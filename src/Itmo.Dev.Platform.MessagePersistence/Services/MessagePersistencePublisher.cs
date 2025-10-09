using Itmo.Dev.Platform.MessagePersistence.Execution;
using Itmo.Dev.Platform.MessagePersistence.Execution.FailureProcessors;
using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Services;

internal class MessagePersistencePublisher : IMessagePersistencePublisher
{
    private readonly IMessagePersistenceExecutor _executor;
    private readonly IMessagePersistenceBufferingExecutor _bufferingExecutor;
    private readonly MessagePersistenceRegistry _registry;

    public MessagePersistencePublisher(
        IMessagePersistenceExecutor executor,
        IMessagePersistenceBufferingExecutor bufferingExecutor,
        MessagePersistenceRegistry registry)
    {
        _executor = executor;
        _bufferingExecutor = bufferingExecutor;
        _registry = registry;
    }

    public async Task PublishAsync(SerializedMessage[] messages, CancellationToken cancellationToken)
    {
        var groupedMessages = messages.WindowedGroupBy(message =>
        (
            message.Name,
            message.BufferingStep,
            NextBufferingStep: GetNextBufferingStep(message)
        ));

        foreach (var messageGroup in groupedMessages)
        {
            if (messageGroup.Key.NextBufferingStep is null)
            {
                var executionConfiguration = GetExecutionConfiguration(
                    messageGroup.Key.Name,
                    messageGroup.Key.BufferingStep);

                await _executor.ExecuteAsync(
                    messageGroup.Key.Name,
                    executionConfiguration,
                    messageGroup,
                    cancellationToken);
            }
            else
            {
                await _bufferingExecutor.ExecuteAsync(
                    messageGroup.Key.Name,
                    messageGroup.Key.NextBufferingStep,
                    messageGroup,
                    cancellationToken);
            }
        }
    }

    private MessagePersistenceExecutionConfiguration GetExecutionConfiguration(
        string messageName,
        string? bufferingStep)
    {
        var record = _registry.GetRecord(messageName);

        if (record.BufferGroup is null || bufferingStep is null)
            return new MessagePersistenceExecutionConfiguration(new RetryMessageHandleFailureProcessor());

        var bufferGroup = _registry.GetBufferingGroup(record.BufferGroup);
        var step = bufferGroup.FindStep(bufferingStep);

        return new MessagePersistenceExecutionConfiguration(
            step?.FailureProcessor ?? new RetryMessageHandleFailureProcessor());
    }

    private string? GetNextBufferingStep(SerializedMessage message)
    {
        var record = _registry.GetRecord(message.Name);

        if (record.BufferGroup is null)
            return null;

        var bufferGroup = _registry.GetBufferingGroup(record.BufferGroup);
        return bufferGroup.FindNextStepName(message.BufferingStep);
    }
}
