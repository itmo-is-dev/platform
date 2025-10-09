using Itmo.Dev.Platform.Common.DateTime;
using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Persistence;
using Itmo.Dev.Platform.Persistence.Abstractions.Transactions;
using Newtonsoft.Json;
using System.Data;
using System.Runtime.CompilerServices;

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
        await foreach (var _ in ConsumeInternalAsync(messageName, messages, cancellationToken)) { }
    }

    public async IAsyncEnumerable<long> ConsumeInternalAsync<TKey, TValue>(
        string messageName,
        IReadOnlyCollection<PersistedMessage<TKey, TValue>> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (messages.Count is 0)
            yield break;

        var createdAt = _dateTimeProvider.Current;

        var serializedMessages = messages
            .Select(message => new SerializedMessage(
                id: default,
                name: messageName,
                createdAt: createdAt,
                state: MessageState.Pending,
                key: JsonConvert.SerializeObject(message.Key, _serializerSettings),
                value: JsonConvert.SerializeObject(message.Value, _serializerSettings),
                retryCount: 0,
                bufferingStep: null))
            .ToArray();

        await using var transaction = await _transactionProvider
            .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var messageIds = _messagePersistenceRepository.AddAsync(serializedMessages, cancellationToken);

        await foreach (var messageId in messageIds)
        {
            yield return messageId;
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
