using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.Common.BackgroundServices;
using Itmo.Dev.Platform.Common.DateTime;
using Itmo.Dev.Platform.Common.Lifetime;
using Itmo.Dev.Platform.Persistence.Abstractions.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IsolationLevel = System.Data.IsolationLevel;

namespace Itmo.Dev.Platform.BackgroundTasks.Scheduling;

internal class BackgroundTaskSchedulingService : RestartableBackgroundService
{
    private readonly IOptionsMonitor<BackgroundTaskSchedulingOptions> _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundTaskSchedulingService> _logger;
    private readonly IPlatformLifetime _platformLifetime;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BackgroundTaskSchedulingService(
        IOptionsMonitor<BackgroundTaskSchedulingOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<BackgroundTaskSchedulingService> logger,
        IPlatformLifetime platformLifetime,
        IDateTimeProvider dateTimeProvider)
    {
        _options = options;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _platformLifetime = platformLifetime;
        _dateTimeProvider = dateTimeProvider;
    }

    protected override async Task ExecuteAsync(CancellationTokenSource cts)
    {
        await _platformLifetime.WaitOnInitializedAsync(cts.Token);

        using var _ = _options.OnChange(_ => cts.Cancel());
        var options = _options.CurrentValue;

        while (cts.IsCancellationRequested is false)
        {
            try
            {
                await ExecuteSingleAsync(options, cts.Token);
            }
            catch (Exception e) when (e is not OperationCanceledException and not TaskCanceledException)
            {
                _logger.LogError(e, "Exception occured while scheduling tasks");
            }
        }
    }

    private async Task ExecuteSingleAsync(
        BackgroundTaskSchedulingOptions options,
        CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskInfrastructureRepository>();
        var scheduler = scope.ServiceProvider.GetRequiredService<IBackgroundTaskScheduler>();
        var transactionProvider = scope.ServiceProvider.GetRequiredService<IPersistenceTransactionProvider>();

        while (cancellationToken.IsCancellationRequested is false)
        {
            await Task.Delay(options.PollingDelay, cancellationToken);

            await using var transaction = await transactionProvider.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

            var query = BackgroundTaskQuery.Build(
                builder => builder
                    .WithState(BackgroundTaskState.Pending)
                    .WithState(BackgroundTaskState.Retrying)
                    .WithState(BackgroundTaskState.Proceeded)
                    .WithCursor(DateTimeOffset.UnixEpoch)
                    .WithMaxScheduledAt(_dateTimeProvider.Current)
                    .WithPageSize(options.BatchSize));

            var backgroundTaskIds = await repository
                .SearchIdsAsync(query, cancellationToken)
                .ToArrayAsync(cancellationToken);

            await repository.UpdateStateAsync(backgroundTaskIds, BackgroundTaskState.Enqueued, cancellationToken);
            await scheduler.ScheduleAsync(backgroundTaskIds, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
    }
}
