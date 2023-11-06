using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.Common.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Transactions;
using BackgroundTaskQuery = Itmo.Dev.Platform.BackgroundTasks.Models.BackgroundTaskQuery;

namespace Itmo.Dev.Platform.BackgroundTasks.Scheduling;

internal class BackgroundTaskSchedulingService : RestartableBackgroundService
{
    private readonly IOptionsMonitor<BackgroundTaskSchedulingOptions> _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundTaskSchedulingService> _logger;

    public BackgroundTaskSchedulingService(
        IOptionsMonitor<BackgroundTaskSchedulingOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<BackgroundTaskSchedulingService> logger)
    {
        _options = options;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationTokenSource cts)
    {
        await Task.Yield();

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

        while (cancellationToken.IsCancellationRequested is false)
        {
            using var transaction = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);

            var query = BackgroundTaskQuery.Build(builder => builder
                .WithState(BackgroundTaskState.Pending)
                .WithState(BackgroundTaskState.Retrying)
                .WithCursor(DateTimeOffset.UnixEpoch)
                .WithPageSize(options.BatchSize));

            var backgroundTaskIds = await repository
                .SearchIdsAsync(query, cancellationToken)
                .ToArrayAsync(cancellationToken);

            await scheduler.ScheduleAsync(backgroundTaskIds, cancellationToken);
            await repository.UpdateStateAsync(backgroundTaskIds, BackgroundTaskState.Enqueued, cancellationToken);

            transaction.Complete();

            await Task.Delay(options.PollingDelay, cancellationToken);
        }
    }
}