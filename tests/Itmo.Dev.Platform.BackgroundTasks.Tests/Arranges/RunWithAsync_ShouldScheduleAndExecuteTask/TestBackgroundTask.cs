using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldScheduleAndExecuteTask;

public class TestBackgroundTask : IBackgroundTask<
    TestBackgroundTaskMetadata,
    EmptyExecutionMetadata,
    TestBackgroundTaskResult,
    EmptyError>
{
    private readonly CompletionManager _completionManager;

    public TestBackgroundTask(CompletionManager completionManager)
    {
        _completionManager = completionManager;
    }

    public static string Name => nameof(TestBackgroundTask);

    public Task<BackgroundTaskExecutionResult<TestBackgroundTaskResult, EmptyError>> ExecuteAsync(
        BackgroundTaskExecutionContext<TestBackgroundTaskMetadata, EmptyExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        var metadata = executionContext.Metadata;
        _completionManager.Complete(metadata.Value);

        return Task.FromResult<BackgroundTaskExecutionResult<TestBackgroundTaskResult, EmptyError>>(
            BackgroundTaskExecutionResult
                .Success
                .WithResult(new TestBackgroundTaskResult(metadata.Value))
                .ForEmptyError());
    }
}