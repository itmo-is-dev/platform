using Itmo.Dev.Platform.MessagePersistence.Execution;
using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Services;

internal class MessagePersistencePublisher : IMessagePersistencePublisher
{
    private readonly IMessagePersistenceExecutor _executor;
    private readonly IMessagePersistenceBufferingExecutor _bufferingExecutor;

    public MessagePersistencePublisher(
        IMessagePersistenceExecutor executor,
        IMessagePersistenceBufferingExecutor bufferingExecutor)
    {
        _executor = executor;
        _bufferingExecutor = bufferingExecutor;
    }

    public async Task PublishAsync(SerializedMessage[] messages, CancellationToken cancellationToken)
    {
        var groupedMessages = messages.WindowedGroupBy(message => (message.Name, message.BufferingStep));

        foreach (var messageGroup in groupedMessages)
        {
            if (messageGroup.Key.BufferingStep is null)
            {
                await _executor.ExecuteAsync(messageGroup.Key.Name, messageGroup, cancellationToken);
            }
            else
            {
                await _bufferingExecutor.ExecuteAsync(
                    messageGroup.Key.Name,
                    messageGroup.Key.BufferingStep,
                    messageGroup,
                    cancellationToken);
            }
        }
    }
}
