using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Internal.Execution.FailureProcessors;
using Itmo.Dev.Platform.MessagePersistence.Internal.Factory;
using Itmo.Dev.Platform.MessagePersistence.Internal.Metrics;
using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Itmo.Dev.Platform.MessagePersistence.Internal.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Internal.Services;
using Itmo.Dev.Platform.MessagePersistence.Internal.Tools;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Execution;

internal class MessagePersistenceExecutor : IMessagePersistenceExecutor
{
    private static readonly ConcurrentDictionary<Type, Type> ExecutorTypeCache = [];

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
        IEnumerable<PersistedMessageModel> messages,
        CancellationToken cancellationToken)
    {
        var record = _registry.GetRecord(messageName);

        var executorType = ExecutorTypeCache.GetOrAdd(
            record.MessageType,
            tuple => typeof(TypedMessagePersistenceExecutor<>).MakeGenericType(tuple));

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
        IReadOnlyDictionary<long, PersistedMessageModel> serializedMessages,
        CancellationToken cancellationToken);
}

[DebuggerDisplay("{ToString()}", Type = "MessagePersistenceExecutor")]
file class TypedMessagePersistenceExecutor<TMessage> : ITypedMessagePersistenceExecutor
    where TMessage : IPersistedMessage
{
    private readonly ILogger<TypedMessagePersistenceExecutor<TMessage>> _logger;
    private readonly IOptionsMonitor<MessagePersistenceHandlerOptions> _handlerOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessagePersistenceInternalRepository _repository;
    private readonly IPersistedMessageFactory _persistedMessageFactory;
    private readonly IMessagePersistenceMetrics _metrics;

    public TypedMessagePersistenceExecutor(
        ILogger<TypedMessagePersistenceExecutor<TMessage>> logger,
        IOptionsMonitor<MessagePersistenceHandlerOptions> handlerOptions,
        IServiceProvider serviceProvider,
        IMessagePersistenceInternalRepository repository,
        IPersistedMessageFactory persistedMessageFactory,
        IMessagePersistenceMetrics metrics)
    {
        _logger = logger;
        _handlerOptions = handlerOptions;
        _serviceProvider = serviceProvider;
        _repository = repository;
        _persistedMessageFactory = persistedMessageFactory;
        _metrics = metrics;
    }

    public async Task ExecuteAsync(
        string messageName,
        MessagePersistenceExecutionConfiguration configuration,
        IReadOnlyDictionary<long, PersistedMessageModel> serializedMessages,
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

        var messages = await CreateMessages(
                messageName,
                serializedMessages.Values,
                handlerOptions,
                cancellationToken)
            .ToArrayAsync(cancellationToken);

        var handler = _serviceProvider.GetRequiredKeyedService<IPersistedMessageHandler<TMessage>>(messageName);

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

            foreach (PersistedMessageReference<TMessage> message in messages)
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

        foreach (PersistedMessageReference<TMessage> message in messages)
        {
            if (message.Result is MessageHandleResult.Success)
            {
                serializedMessages[message.Id].State = MessageState.Completed;
                _metrics.IncMessageFinishedSuccessfully(messageName);
            }
            else if (message.Result is MessageHandleResult.Ignored)
            {
                serializedMessages[message.Id].State = MessageState.Pending;
                _metrics.IncMessageIgnored(messageName);
            }
            else if (message.Result is MessageHandleResult.Failure failure)
            {
                configuration.FailureProcessor.Process(
                    failureProcessorContext,
                    failure,
                    serializedMessages[message.Id]);

                _metrics.IncMessageFailed(messageName);

                _logger.LogWarning(
                    failure.Exception,
                    "Failed to process message '{MessageName}', id: '{MessageId}'",
                    messageName,
                    message.Id);
            }
        }

        // Caveat for future implementations of buffering that rely on DB:
        // If FailureProcessor is configured to throw exceptions and DB buffering implementation opens transaction
        // before calling the publisher, this call would not be commited.
        // This means that buffering retries would retry messages that are effectively succeeded if they are processed
        // in batch with failed messages.
        await _repository.UpdateAsync(serializedMessages.Values.ToArray(), cancellationToken);
    }

    private async IAsyncEnumerable<PersistedMessageReference<TMessage>> CreateMessages(
        string messageName,
        IEnumerable<PersistedMessageModel> messages,
        MessagePersistenceHandlerOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (PersistedMessageModel messageModel in messages)
        {
            TMessage message;

            try
            {
                message = (TMessage)await _persistedMessageFactory.CreateAsync(
                    messageModel,
                    cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogWarning(
                    e,
                    "Failed to deserialize persisted message value. Message name = {MessageName}, id = {Id}, key = {Value}",
                    messageName,
                    messageModel.Id,
                    messageModel.Value);

                messageModel.State = MessageState.Failed;

                continue;
            }

            MessageHandleResult result = options.DefaultHandleResult switch
            {
                MessageHandleResultKind.Success => new MessageHandleResult.Success(),
                MessageHandleResultKind.Failure => new MessageHandleResult.Failure(null),
                MessageHandleResultKind.Ignored => new MessageHandleResult.Ignored(),
                _ => throw new UnreachableException(),
            };

            yield return new PersistedMessageReference<TMessage>(messageModel.Id,
                messageModel.CreatedAt,
                message,
                result);
        }
    }
}
