using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Common.Lifetime;
using Itmo.Dev.Platform.MessagePersistence.Internal.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Itmo.Dev.Platform.MessagePersistence.Internal.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Internal.Tools;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Itmo.Dev.Platform.Persistence.Abstractions.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Diagnostics;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Services;

internal class MessagePersistenceInitialPublishBackgroundService : BackgroundService
{
    private readonly string _publisherName;
    private readonly MessagePersistencePublisherOptions _publisherOptions;
    private readonly ILogger<MessagePersistenceInitialPublishBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IPlatformLifetime _platformLifetime;

    public MessagePersistenceInitialPublishBackgroundService(
        string publisherName,
        IOptionsMonitor<MessagePersistencePublisherOptions> options,
        ILogger<MessagePersistenceInitialPublishBackgroundService> logger,
        IServiceScopeFactory scopeFactory,
        IPlatformLifetime platformLifetime)
    {
        _publisherName = publisherName;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _platformLifetime = platformLifetime;
        _publisherOptions = options.Get(publisherName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        await _platformLifetime.WaitOnInitializedAsync(stoppingToken);

        while (stoppingToken.IsCancellationRequested is false)
        {
            try
            {
                await ExecuteSingleAsync(stoppingToken);
            }
            catch (Exception e) when (e is not TaskCanceledException and not OperationCanceledException)
            {
                _logger.LogError(e, "Error while publishing from '{PublisherName}'", _publisherName);
            }
        }
    }

    private async Task ExecuteSingleAsync(CancellationToken cancellationToken)
    {
        if (_publisherOptions.MessageNames is [])
            await Task.Delay(Timeout.Infinite, cancellationToken);

        var query = PersistedMessageQuery.Build(builder => builder
            .WithNames(_publisherOptions.MessageNames)
            .WithState(MessageState.Pending)
            .WithCursor(DateTimeOffset.MinValue)
            .WithPageSize(_publisherOptions.BatchSize));

        var shouldDelayBeforePolling = false;

        while (cancellationToken.IsCancellationRequested is false)
        {
            if (shouldDelayBeforePolling)
            {
                await Task.Delay(_publisherOptions.PollingDelay, cancellationToken);
            }
            else
            {
                shouldDelayBeforePolling = true;
            }

            await using var scope = _scopeFactory.CreateAsyncScope();

            var repository = scope.ServiceProvider.GetRequiredService<IMessagePersistenceInternalRepository>();
            var transactionProvider = scope.ServiceProvider.GetRequiredService<IPersistenceTransactionProvider>();

            var publisher = scope.ServiceProvider.GetRequiredService<IMessagePersistencePublisher>();

            await using var transaction = await transactionProvider.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

            var serializedMessages = await repository
                .QueryAsync(query, cancellationToken)
                .Select(ProcessMessage)
                .ToArrayAsync(cancellationToken);

            _logger.LogInformation(
                "Polling persisted messages from publisher '{PublisherName}', received {Count} messages",
                _publisherName,
                serializedMessages.Length);

            if (serializedMessages is not [])
            {
                using var activity = MessagePersistenceActivitySource.Value
                    .StartActivity(
                        name: MessagePersistenceConstants.Tracing.SpanName,
                        ActivityKind.Internal,
                        parentContext: default,
                        links: serializedMessages.SelectMany(message => message.GetActivityLinks()))
                    .WithDisplayName("[publish]");
                
                await publisher.PublishAsync(serializedMessages, cancellationToken);
            }
            else
            {
                shouldDelayBeforePolling = true;
            }

            await transaction.CommitAsync(cancellationToken);
        }
    }

    private PersistedMessageModel ProcessMessage(PersistedMessageModel message)
    {
        message.BufferingStep = null;
        return message;
    }
}
