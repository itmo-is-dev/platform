using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldScheduleAndExecuteTask;

public class TestBackgroundTask :
    IBackgroundTask<TestBackgroundTaskMetadata, TestBackgroundTaskResult, EmptyError>
{
    private readonly CompletionManager _completionManager;

    public TestBackgroundTask(CompletionManager completionManager)
    {
        _completionManager = completionManager;
    }

    public static string Name => nameof(TestBackgroundTask);

    public async Task<BackgroundTaskExecutionResult<TestBackgroundTaskResult, EmptyError>> ExecuteAsync(
        BackgroundTaskExecutionContext<TestBackgroundTaskMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        var metadata = executionContext.Metadata;
        _completionManager.Complete(metadata.Value);

        return new BackgroundTaskExecutionResult<TestBackgroundTaskResult, EmptyError>.Success(
            new TestBackgroundTaskResult(metadata.Value));
    }
}