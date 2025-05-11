using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.Common.DateTime;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.
    ScheduleWithAsync_ShouldScheduleTask_OnlyWhenScheduleTimeArrives;

public class ScheduledAtBackgroundTask : IBackgroundTask<
    ScheduledAtMetadata,
    EmptyExecutionMetadata,
    EmptyExecutionResult,
    EmptyError>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public ScheduledAtBackgroundTask(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public static string Name => nameof(ScheduledAtBackgroundTask);

    public async Task<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>> ExecuteAsync(
        BackgroundTaskExecutionContext<ScheduledAtMetadata, EmptyExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        return _dateTimeProvider.Current < executionContext.Metadata.ScheduledAt
            ? BackgroundTaskExecutionResult.Failure.ForEmptyResult().WithEmptyError()
            : BackgroundTaskExecutionResult.Success.WithEmptyResult().ForEmptyError();
    }
}
