using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.Common.DateTime;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.SuspendedUntil_ShouldProceedOnItsOwn_WhenScheduleTimeArrives;

public class SuspendedUntilBackgroundTask : IBackgroundTask<
    SuspendedUntilMetadata,
    SuspendedUntilExecutionMetadata,
    EmptyExecutionResult,
    EmptyError>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public SuspendedUntilBackgroundTask(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public static string Name => nameof(SuspendedUntilBackgroundTask);

    public async Task<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>> ExecuteAsync(
        BackgroundTaskExecutionContext<SuspendedUntilMetadata, SuspendedUntilExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        if (executionContext.ExecutionMetadata.WasSuspended is false)
        {
            executionContext.ExecutionMetadata.WasSuspended = true;

            return BackgroundTaskExecutionResult.Suspended
                .ForEmptyResult()
                .ForEmptyError()
                .Until(executionContext.Metadata.ContinuationScheduled);
        }

        return _dateTimeProvider.Current < executionContext.Metadata.ContinuationScheduled
            ? BackgroundTaskExecutionResult.Failure.ForEmptyResult().WithEmptyError()
            : BackgroundTaskExecutionResult.Success.WithEmptyResult().ForEmptyError();
    }
}
