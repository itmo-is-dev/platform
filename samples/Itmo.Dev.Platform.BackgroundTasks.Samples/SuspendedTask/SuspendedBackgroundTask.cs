using Itmo.Dev.Platform.BackgroundTasks.Samples.SimpleTask;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.SuspendedTask;

public class SuspendedBackgroundTask : IBackgroundTask<
    SuspendedTaskMetadata,
    SuspendedTaskExecutionMetadata,
    EmptyExecutionResult,
    EmptyError>
{
    public static string Name => nameof(SimpleBackgroundTask);

    public Task<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>> ExecuteAsync(
        BackgroundTaskExecutionContext<SuspendedTaskMetadata, SuspendedTaskExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        if (executionContext.ExecutionMetadata.Count is 6)
        {
            return Task.FromResult<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>>(
                BackgroundTaskExecutionResult.Success.WithEmptyResult().ForEmptyError());
        }

        executionContext.ExecutionMetadata.Count++;

        return Task.FromResult<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>>(
            BackgroundTaskExecutionResult.Suspended.ForEmptyResult().ForEmptyError());
    }
}