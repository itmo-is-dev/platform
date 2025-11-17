using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Exceptions;
using Itmo.Dev.Platform.MessagePersistence.Execution.FailureProcessors;
using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Itmo.Dev.Platform.MessagePersistence.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Services;
using Itmo.Dev.Platform.MessagePersistence.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Transactions;

namespace Itmo.Dev.Platform.MessagePersistence.Execution;

#pragma warning disable CA1506

internal class MessagePersistenceExecutor : IMessagePersistenceExecutor
{
    private static readonly ConcurrentDictionary<(Type KeyType, Type ValueType), Type> ExecutorTypeCache = [];

    private readonly MessagePersistenceRegistry _registry;
    private readonly IServiceProvider _serviceProvider;

    public MessagePersistenceExecutor(MessagePersistenceRegistry registry, IServiceProvider serviceProvider)
    {
        _registry = registry;
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(
        string messageName,
        MessagePersistenceExecutionConfiguration configuration,
        IEnumerable<SerializedMessage> messages,
        CancellationToken cancellationToken)
    {
        var record = _registry.GetRecord(messageName);

        var executorType = ExecutorTypeCache.GetOrAdd(
            (record.KeyType, record.ValueType),
            tuple => typeof(TypedMessagePersistenceExecutor<,>).MakeGenericType(tuple.KeyType, tuple.ValueType));

        var executor = (ITypedMessagePersistenceExecutor)ActivatorUtilities.CreateInstance(
            _serviceProvider,
            executorType);

        await executor.ExecuteAsync(messageName, configuration, messages.ToDictionary(x => x.Id), cancellationToken);
    }
}

file interface ITypedMessagePersistenceExecutor
{
    Task ExecuteAsync(
        string messageName,
        MessagePersistenceExecutionConfiguration configuration,
        IReadOnlyDictionary<long, SerializedMessage> serializedMessages,
        CancellationToken cancellationToken);
}

[DebuggerDisplay("{ToString()}", Type = "MessagePersistenceExecutor")]
file class TypedMessagePersistenceExecutor<TKey, TValue> : ITypedMessagePersistenceExecutor
{
    private readonly ILogger<TypedMessagePersistenceExecutor<TKey, TValue>> _logger;
    private readonly IOptionsMonitor<MessagePersistenceHandlerOptions> _handlerOptions;
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessagePersistenceInternalRepository _repository;

    public TypedMessagePersistenceExecutor(
        ILogger<TypedMessagePersistenceExecutor<TKey, TValue>> logger,
        IOptionsMonitor<MessagePersistenceHandlerOptions> handlerOptions,
        JsonSerializerSettings serializerSettings,
        IServiceProvider serviceProvider,
        IMessagePersistenceInternalRepository repository)
    {
        _logger = logger;
        _handlerOptions = handlerOptions;
        _serializerSettings = serializerSettings;
        _serviceProvider = serviceProvider;
        _repository = repository;
    }

    public async Task ExecuteAsync(
        string messageName,
        MessagePersistenceExecutionConfiguration configuration,
        IReadOnlyDictionary<long, SerializedMessage> serializedMessages,
        CancellationToken cancellationToken)
    {
        // Do not add activity links here, it is assumed that the caller propagated links/traces at this point.
        // Responsibility to "connect" span links is not designated to this service, as trace parent that may be stored
        // in messages could be way before than the current spans.
        // So we intentionally "propagate" span links at transport roots (such as initial publish service), and
        // propagate them further via transport (such as kafka).
        using var activity = MessagePersistenceActivitySource.Value
            .StartActivity(
                name: MessagePersistenceConstants.Tracing.SpanName,
                ActivityKind.Internal,
                parentContext: default)
            .WithDisplayName($"[handle] {messageName}");

        var handlerOptions = _handlerOptions.Get(messageName);

        var messages = MapMessages(
                messageName,
                serializedMessages.Values,
                handlerOptions)
            .ToArray();

        var handler = _serviceProvider.GetRequiredKeyedService<IMessagePersistenceHandler<TKey, TValue>>(messageName);

        try
        {
            // Here we suppress parent transaction from our library/pipeline (if exists) so the changes commited
            // in a handler would not be reverted if some other messages would produce throwing behaviour and cancel
            // parent transaction (again, if exists).
            // This allows package consumers to implement idempotency on their own as we do not interfere with their
            // transactions.
            using var _ = new TransactionScope(
                TransactionScopeOption.Suppress,
                TransactionScopeAsyncFlowOption.Enabled);

            await handler.HandleAsync(messages, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while executing persistence messages = {MessageName}", messageName);

            foreach (Message<TKey, TValue> message in messages)
            {
                message.SetFailedResult(e);
            }
        }

        // Scoped using is here by design.
        // We expect exceptions here only in buffering context, as FailureProcessor is configured on a
        // per buffering step basis.
        // We do not expect any DB transactions in a buffering context.
        // We need to block buffer processing so it would retry on its own, not sending messages again through the
        // pipeline.
        // We don't need to retry already completed messages, buffering implementations are expected to skip messages
        // that are not published (Completed/Pending in this context).
        // We are using scoped using here to update message states (and other attributes) no matter if we block buffer
        // or not.
        using var failureProcessorContext = new MessageHandleFailureProcessorContext(handlerOptions);

        foreach (Message<TKey, TValue> message in messages)
        {
            if (message.Result is MessageHandleResult.Success)
            {
                serializedMessages[message.Id].State = MessageState.Completed;
            }
            else if (message.Result is MessageHandleResult.Ignored)
            {
                serializedMessages[message.Id].State = MessageState.Pending;
            }
            else if (message.Result is MessageHandleResult.Failure failure)
            {
                configuration.FailureProcessor.Process(
                    failureProcessorContext,
                    failure,
                    serializedMessages[message.Id]);
            }
        }

        // Caveat for future implementations of buffering that rely on DB:
        // If FailureProcessor is configured to throw exceptions and DB buffering implementation opens transaction
        // before calling the publisher, this call would not be commited.
        // This means that buffering retries would retry messages that are effectively succeeded if they are processed
        // in batch with failed messages.
        await _repository.UpdateAsync(serializedMessages.Values.ToArray(), cancellationToken);
    }

    private IEnumerable<Message<TKey, TValue>> MapMessages(
        string messageName,
        IEnumerable<SerializedMessage> messages,
        MessagePersistenceHandlerOptions options)
    {
        foreach (SerializedMessage message in messages)
        {
            var key = JsonConvert.DeserializeObject<TKey>(message.Key, _serializerSettings);

            if (key is null)
            {
                _logger.LogWarning(
                    "Failed to deserialize persisted message key. Message name = {MessageName}, id = {Id}, key = {Key}",
                    messageName,
                    message.Id,
                    message.Key);

                message.State = MessageState.Failed;

                continue;
            }

            var value = JsonConvert.DeserializeObject<TValue>(message.Value, _serializerSettings);

            if (value is null)
            {
                _logger.LogWarning(
                    "Failed to deserialize persisted message value. Message name = {MessageName}, id = {Id}, key = {Value}",
                    messageName,
                    message.Id,
                    message.Value);

                message.State = MessageState.Failed;

                continue;
            }

            MessageHandleResult result = options.DefaultHandleResult switch
            {
                MessageHandleResultKind.Success => new MessageHandleResult.Success(),
                MessageHandleResultKind.Failure => new MessageHandleResult.Failure(null),
                MessageHandleResultKind.Ignored => new MessageHandleResult.Ignored(),
                _ => throw new UnreachableException(),
            };

            yield return new Message<TKey, TValue>(
                message.Id,
                message.CreatedAt,
                key,
                value,
                result);
        }
    }
}
