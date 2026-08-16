using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldSetStateFailed_WhenRetryCountExceeded;

public class FailingBackgroundTask : IBackgroundTask<
    ValueMetadata,
    EmptyExecutionMetadata,
    EmptyExecutionResult,
    EmptyError>
{
    public static string Name => nameof(FailingBackgroundTask);

    public Task<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>> ExecuteAsync(
        BackgroundTaskExecutionContext<ValueMetadata, EmptyExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(executionContext.Metadata.Value))
            throw new ArgumentException("Invalid metadata, possible serialization issues");

        
        return Task.FromResult<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>>(
            BackgroundTaskExecutionResult.Failure.ForEmptyResult().WithEmptyError());
    }
}