using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Itmo.Dev.Platform.MessagePersistence.Internal.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Internal.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itmo.Dev.Platform.Kafka.MessagePersistence;

internal class KafkaBufferingStepConsumerHandler : IKafkaConsumerHandler<BufferedMessageKey, BufferedMessageValue>
{
    private readonly IMessagePersistenceInternalRepository _repository;
    private readonly IMessagePersistencePublisher _publisher;
    private readonly ILogger<KafkaBufferingStepConsumerHandler> _logger;
    private readonly IOptions<JsonSerializerSettings> _serializerSettings;

    public KafkaBufferingStepConsumerHandler(
        IMessagePersistenceInternalRepository repository,
        IMessagePersistencePublisher publisher,
        ILogger<KafkaBufferingStepConsumerHandler> logger,
        IOptions<JsonSerializerSettings> serializerSettings)
    {
        _repository = repository;
        _publisher = publisher;
        _logger = logger;
        _serializerSettings = serializerSettings;
    }

    public async ValueTask HandleAsync(
        IEnumerable<IKafkaConsumerMessage<BufferedMessageKey, BufferedMessageValue>> messages,
        CancellationToken cancellationToken)
    {
        messages = messages.ToArray();
        var messageIds = messages.Select(message => message.Value.Message.Id).ToArray();

        _logger.LogInformation(
            "Handling buffered messages, count = {MessageCount}",
            messageIds.Length);

        var query = InternalPersistedMessageQuery.Build(builder => builder
            .WithIds(messageIds)
            .WithPageSize(messageIds.Length));

        var serializedMessages = await _repository
            .QueryAsync(query, cancellationToken)
            .ToArrayAsync(cancellationToken);

        if (serializedMessages.Length != query.Ids.Length)
        {
            var notFoundMessages = messages
                .Where(m => serializedMessages.All(mm => mm.Id != m.Value.Message.Id))
                .Select(m => m.Value.Message)
                .Select(m => JsonConvert.SerializeObject(m, _serializerSettings.Value));

            _logger.LogError(
                "Failed to find all persisted messages, not found messages = [{Messages}]",
                string.Join(", ", notFoundMessages));
        }

        serializedMessages = FilterMessages(serializedMessages).ToArray();

        await _publisher.PublishAsync(serializedMessages, cancellationToken);
    }

    private IEnumerable<PersistedMessageModel> FilterMessages(IEnumerable<PersistedMessageModel> messages)
    {
        foreach (PersistedMessageModel message in messages)
        {
            if (message.State is MessageState.Published)
            {
                yield return message;
            }
            else
            {
                _logger.LogInformation(
                    "Skipping message = {MessageId} publication due to incorrect state = {MessageState}",
                    message.Id,
                    message.State);
            }
        }
    }
}
