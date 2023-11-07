using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldSetStateFailed_WhenRetryCountExceeded;

public class FailingBackgroundTask :
    IBackgroundTask<EmptyMetadata, EmptyExecutionMetadata, EmptyExecutionResult, EmptyError>
{
    public static string Name => nameof(FailingBackgroundTask);

    public async Task<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>> ExecuteAsync(
        BackgroundTaskExecutionContext<EmptyMetadata, EmptyExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        return new BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>.Failure(EmptyError.Value);
    }
}