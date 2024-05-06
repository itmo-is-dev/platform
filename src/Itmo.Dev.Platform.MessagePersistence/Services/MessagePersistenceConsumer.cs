using Itmo.Dev.Platform.Common.DateTime;
using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Persistence;
using Itmo.Dev.Platform.Persistence.Abstractions.Transactions;
using Newtonsoft.Json;
using System.Data;

namespace Itmo.Dev.Platform.MessagePersistence.Services;

internal class MessagePersistenceConsumer : IMessagePersistenceConsumer
{
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPersistenceTransactionProvider _transactionProvider;
    private readonly IMessagePersistenceInternalRepository _messagePersistenceRepository;

    public MessagePersistenceConsumer(
        JsonSerializerSettings serializerSettings,
        IDateTimeProvider dateTimeProvider,
        IPersistenceTransactionProvider transactionProvider,
        IMessagePersistenceInternalRepository messagePersistenceRepository)
    {
        _serializerSettings = serializerSettings;
        _dateTimeProvider = dateTimeProvider;
        _transactionProvider = transactionProvider;
        _messagePersistenceRepository = messagePersistenceRepository;
    }

    public async ValueTask ConsumeAsync<TKey, TValue>(
        string messageName,
        IReadOnlyCollection<PersistedMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken)
    {
        if (messages.Count is 0)
            return;

        var createdAt = _dateTimeProvider.Current;

        var serializedMessages = messages
            .Select(message => new SerializedMessage(
                Id: default,
                Name: messageName,
                CreatedAt: createdAt,
                State: MessageState.Pending,
                Key: JsonConvert.SerializeObject(message.Key, _serializerSettings),
                Value: JsonConvert.SerializeObject(message.Value, _serializerSettings),
                RetryCount: 0))
            .ToArray();

        await using var transaction = await _transactionProvider
            .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        await _messagePersistenceRepository.AddAsync(serializedMessages, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}