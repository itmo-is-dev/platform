using Itmo.Dev.Platform.MessagePersistence.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Itmo.Dev.Platform.MessagePersistence.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Itmo.Dev.Platform.MessagePersistence.Execution;

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

        await executor.ExecuteAsync(messageName, messages.ToArray(), cancellationToken);
    }
}

file interface ITypedMessagePersistenceExecutor
{
    Task ExecuteAsync(
        string messageName,
        SerializedMessage[] serializedMessages,
        CancellationToken cancellationToken);
}

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
        SerializedMessage[] serializedMessages,
        CancellationToken cancellationToken)
    {
        var handlerOptions = _handlerOptions.Get(messageName);

        var messageStates = serializedMessages.ToDictionary(x => x.Id, x => x.State);
        var messageRetryCounts = serializedMessages.ToDictionary(x => x.Id, x => x.RetryCount);

        var messages = MapMessages(messageName, serializedMessages, messageStates, handlerOptions).ToArray();

        var handler = _serviceProvider.GetRequiredKeyedService<IMessagePersistenceHandler<TKey, TValue>>(messageName);

        try
        {
            await handler.HandleAsync(messages, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while executing persistence messages = {MessageName}", messageName);

            foreach (Message<TKey, TValue> message in messages)
            {
                message.SetResult(MessageHandleResult.Failure);
            }
        }

        foreach (Message<TKey, TValue> message in messages)
        {
            messageStates[message.Id] = message.Result switch
            {
                MessageHandleResult.Success => MessageState.Completed,
                MessageHandleResult.Failure => MessageState.Failed,
                MessageHandleResult.Ignored => MessageState.Pending,
                _ => throw new UnreachableException(),
            };

            if (message.Result is MessageHandleResult.Failure)
            {
                var retryCount = ++messageRetryCounts[message.Id];

                if (retryCount < handlerOptions.RetryCount)
                {
                    messageStates[message.Id] = MessageState.Pending;
                }
            }
        }

        for (var i = 0; i < serializedMessages.Length; i++)
        {
            SerializedMessage message = serializedMessages[i];

            serializedMessages[i] = serializedMessages[i] with
            {
                State = messageStates[message.Id],
                RetryCount = messageRetryCounts[message.Id],
            };
        }

        await _repository.UpdateAsync(serializedMessages, cancellationToken);
    }

    private IEnumerable<Message<TKey, TValue>> MapMessages(
        string messageName,
        IEnumerable<SerializedMessage> messages,
        IDictionary<long, MessageState> states,
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

                states[message.Id] = MessageState.Failed;

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

                states[message.Id] = MessageState.Failed;

                continue;
            }

            yield return new Message<TKey, TValue>(
                message.Id,
                message.CreatedAt,
                key,
                value,
                options.DefaultHandleResult);
        }
    }
}
