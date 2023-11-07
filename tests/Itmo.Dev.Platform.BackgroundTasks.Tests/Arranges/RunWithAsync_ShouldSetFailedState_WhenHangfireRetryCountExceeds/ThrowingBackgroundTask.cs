using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.
    RunWithAsync_ShouldSetFailedState_WhenHangfireRetryCountExceeds;

public class ThrowingBackgroundTask :
    IBackgroundTask<EmptyMetadata, EmptyExecutionMetadata, EmptyExecutionResult, EmptyError>
{
    public static string Name => nameof(ThrowingBackgroundTask);

    public Task<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>> ExecuteAsync(
        BackgroundTaskExecutionContext<EmptyMetadata, EmptyExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Task cannot be executed");
    }
}