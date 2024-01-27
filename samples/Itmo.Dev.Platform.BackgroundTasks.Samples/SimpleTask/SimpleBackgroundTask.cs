using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.SimpleTask;

public class SimpleBackgroundTask : IBackgroundTask<
    SimpleTaskMetadata,
    EmptyExecutionMetadata,
    SimpleTaskResult,
    EmptyError>
{
    public static string Name => nameof(SimpleBackgroundTask);

    public Task<BackgroundTaskExecutionResult<SimpleTaskResult, EmptyError>> ExecuteAsync(
        BackgroundTaskExecutionContext<SimpleTaskMetadata, EmptyExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        var result = string.Join(", ", executionContext.Metadata.Values);

        return Task.FromResult<BackgroundTaskExecutionResult<SimpleTaskResult, EmptyError>>(
            BackgroundTaskExecutionResult.Success.WithResult(new SimpleTaskResult(result)).ForEmptyError());
    }
}