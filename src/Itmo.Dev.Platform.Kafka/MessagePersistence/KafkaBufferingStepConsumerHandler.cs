using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Services;

namespace Itmo.Dev.Platform.Kafka.MessagePersistence;

internal class KafkaBufferingStepConsumerHandler : IKafkaConsumerHandler<BufferedMessageKey, BufferedMessageValue>
{
    private readonly IMessagePersistenceInternalRepository _repository;
    private readonly IMessagePersistencePublisher _publisher;

    public KafkaBufferingStepConsumerHandler(
        IMessagePersistenceInternalRepository repository,
        IMessagePersistencePublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async ValueTask HandleAsync(
        IEnumerable<IKafkaConsumerMessage<BufferedMessageKey, BufferedMessageValue>> messages,
        CancellationToken cancellationToken)
    {
        var messageIds = messages.Select(message => message.Value.Message.Id).ToArray();

        var query = SerializedMessageQuery.Build(builder => builder
            .WithIds(messageIds)
            .WithPageSize(messageIds.Length));

        var serializedMessages = await _repository
            .QueryAsync(query, cancellationToken)
            .ToArrayAsync(cancellationToken);

        if (serializedMessages.Length != query.Ids.Length)
            throw new InvalidOperationException("Failed to find all persisted messages");

        await _publisher.PublishAsync(serializedMessages, cancellationToken);
    }
}
