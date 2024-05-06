using Itmo.Dev.Platform.Common.BackgroundServices;
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Common.Lifetime;
using Itmo.Dev.Platform.MessagePersistence.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Persistence;
using Itmo.Dev.Platform.Persistence.Abstractions.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Data;

namespace Itmo.Dev.Platform.MessagePersistence.Services;

internal class MessagePersistenceBackgroundService<TKey, TValue> : RestartableBackgroundService
{
    private readonly string _messageName;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MessagePersistenceBackgroundService<TKey, TValue>> _logger;
    private readonly IPlatformLifetime _platformLifetime;

    public MessagePersistenceBackgroundService(
        string messageName,
        IServiceScopeFactory scopeFactory,
        ILogger<MessagePersistenceBackgroundService<TKey, TValue>> logger,
        IPlatformLifetime platformLifetime)
    {
        _messageName = messageName;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _platformLifetime = platformLifetime;
    }

    protected override async Task ExecuteAsync(CancellationTokenSource cts)
    {
        await Task.Yield();
        await _platformLifetime.WaitOnInitializedAsync(cts.Token);

        await using var scope = _scopeFactory.CreateAsyncScope();

        var monitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<MessagePersistenceHandlerOptions>>();
        using var monitorSubscription = monitor.OnNamedChange(_messageName, _ => cts.Cancel());

        var handlerOptions = monitor.Get(_messageName);

        while (cts.IsCancellationRequested is false)
        {
            try
            {
                await ExecuteSingleAsync(scope.ServiceProvider, handlerOptions, cts.Token);
            }
            catch (Exception e) when (e is not TaskCanceledException and not OperationCanceledException)
            {
                _logger.LogError(e, "Error during processing persisted messages = {MessageName}", _messageName);
            }
        }
    }

    private async Task ExecuteSingleAsync(
        IServiceProvider provider,
        MessagePersistenceHandlerOptions options,
        CancellationToken cancellationToken)
    {
        var repository = provider.GetRequiredService<IMessagePersistenceInternalRepository>();
        var transactionProvider = provider.GetRequiredService<IPersistenceTransactionProvider>();

        var query = SerializedMessageQuery.Build(builder => builder
            .WithName(_messageName)
            .WithState(MessageState.Pending)
            .WithCursor(DateTimeOffset.MinValue)
            .WithPageSize(options.BatchSize));

        while (cancellationToken.IsCancellationRequested is false)
        {
            await using var transaction = await transactionProvider
                .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            var serializedMessages = await repository
                .QueryAsync(query, cancellationToken)
                .ToArrayAsync(cancellationToken);

            if (serializedMessages is [])
            {
                await transaction.CommitAsync(cancellationToken);

                await Task.Delay(options.PollingDelay, cancellationToken);
                continue;
            }

            var messageStates = serializedMessages.ToDictionary(x => x.Id, x => x.State);
            var messageRetryCounts = serializedMessages.ToDictionary(x => x.Id, x => x.RetryCount);

            var messages = MapMessages(serializedMessages, messageStates, options, provider).ToArray();

            await using var scope = _scopeFactory.CreateAsyncScope();

            var handler = scope.ServiceProvider
                .GetRequiredKeyedService<IMessagePersistenceHandler<TKey, TValue>>(_messageName);

            await handler.HandleAsync(messages, cancellationToken);

            foreach (Message<TKey, TValue> message in messages)
            {
                messageStates[message.Id] = message.Result switch
                {
                    MessageHandleResult.Success => MessageState.Completed,
                    MessageHandleResult.Failure => MessageState.Failed,
                    MessageHandleResult.Ignored => MessageState.Pending,
                    _ => throw new ArgumentOutOfRangeException(),
                };

                if (message.Result is MessageHandleResult.Failure)
                {
                    var retryCount = ++messageRetryCounts[message.Id];

                    if (retryCount < options.RetryCount)
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

            await repository.UpdateAsync(serializedMessages, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            await Task.Delay(options.PollingDelay, cancellationToken);
        }
    }

    private IEnumerable<Message<TKey, TValue>> MapMessages(
        IEnumerable<SerializedMessage> messages,
        IDictionary<long, MessageState> states,
        MessagePersistenceHandlerOptions options,
        IServiceProvider provider)
    {
        var serializerSettings = provider.GetRequiredService<JsonSerializerSettings>();

        foreach (SerializedMessage message in messages)
        {
            var key = JsonConvert.DeserializeObject<TKey>(message.Key, serializerSettings);

            if (key is null)
            {
                _logger.LogWarning(
                    "Failed to deserialize persisted message key. Message name = {MessageName}, id = {Id}, key = {Key}",
                    _messageName,
                    message.Id,
                    message.Key);

                states[message.Id] = MessageState.Failed;

                continue;
            }

            var value = JsonConvert.DeserializeObject<TValue>(message.Value, serializerSettings);

            if (value is null)
            {
                _logger.LogWarning(
                    "Failed to deserialize persisted message value. Message name = {MessageName}, id = {Id}, key = {Value}",
                    _messageName,
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