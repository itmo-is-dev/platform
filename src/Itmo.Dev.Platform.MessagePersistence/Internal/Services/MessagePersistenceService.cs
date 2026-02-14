using Itmo.Dev.Platform.Common.DateTime;
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Internal.Metrics;
using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Itmo.Dev.Platform.MessagePersistence.Internal.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Internal.Tools;
using Itmo.Dev.Platform.Persistence.Abstractions.Transactions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Services;

internal class MessagePersistenceService(
    IOptions<JsonSerializerSettings> serializerSettings,
    IDateTimeProvider dateTimeProvider,
    IPersistenceTransactionProvider transactionProvider,
    IMessagePersistenceInternalRepository messagePersistenceRepository,
    MessagePersistenceRegistry registry,
    IMessagePersistenceMetrics metrics
)
    : IMessagePersistenceService
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public async ValueTask PersistAsync<TMessage>(
        IReadOnlyCollection<TMessage> messages,
        CancellationToken cancellationToken)
        where TMessage : IPersistedMessage
    {
        await foreach (var _ in PersistInternalAsync(messages, cancellationToken)) { }
    }

    public async IAsyncEnumerable<long> PersistInternalAsync<TMessage>(
        IReadOnlyCollection<TMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where TMessage : IPersistedMessage
    {
        if (messages.Count is 0)
            yield break;

        var createdAt = dateTimeProvider.Current;

        var messageName = registry.GetMessageName(typeof(TMessage));
        var messageRecord = registry.GetRecord(messageName);

        using var activity = MessagePersistenceActivitySource.Value
            .StartActivity(
                name: MessagePersistenceConstants.Tracing.SpanName,
                ActivityKind.Internal,
                parentContext: default)
            .WithDisplayName($"[persist] {messageName}");

        var headers = new Dictionary<string, string>(EnumerateHeaders());

        var serializedMessages = messages
            .Select(message =>
            {
                if (message.Payload.GetType().IsAssignableTo(messageRecord.PayloadType) is false)
                {
                    throw MessagePersistenceException.InvalidPayloadType(
                        messageRecord.PayloadType,
                        message.Payload.GetType());
                }

                return new PersistedMessageCreateModel
                {
                    Name = messageName,
                    Version = messageRecord.Version,
                    Key = JsonConvert.SerializeObject(message.Payload.Key, serializerSettings.Value),
                    Value = JsonConvert.SerializeObject(message.Payload, serializerSettings.Value),
                    CreatedAt = createdAt,
                    State = MessageState.Pending,
                    Headers = headers,
                };
            })
            .ToArray();

        await using var transaction = await transactionProvider
            .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var messageIds = messagePersistenceRepository.AddAsync(serializedMessages, cancellationToken);

        await foreach (var messageId in messageIds)
        {
            yield return messageId;
        }

        await transaction.CommitAsync(cancellationToken);

        metrics.IncMessageCreated(messages.Count, messageName);
    }

    private static IEnumerable<KeyValuePair<string, string>> EnumerateHeaders()
    {
        if (Activity.Current is { Id: { } traceId })
        {
            yield return new KeyValuePair<string, string>(
                MessagePersistenceConstants.Tracing.TraceParentHeader,
                traceId);
        }
    }
}
