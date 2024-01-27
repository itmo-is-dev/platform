using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldScheduleAndExecuteTask;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.ProceedAsync_ShouldProceedTaskExecution;

public class ProceedableBackgroundTask : IBackgroundTask<
    EmptyMetadata,
    EmptyExecutionMetadata,
    EmptyExecutionResult,
    EmptyError>
{
    private readonly CompletionManager _completionManager;
    private readonly ILogger<ProceedableBackgroundTask> _logger;

    public ProceedableBackgroundTask(CompletionManager completionManager, ILogger<ProceedableBackgroundTask> logger)
    {
        _completionManager = completionManager;
        _logger = logger;
    }

    public static string Name => nameof(ProceedableBackgroundTask);

    public Task<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>> ExecuteAsync(
        BackgroundTaskExecutionContext<EmptyMetadata, EmptyExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        if (_completionManager.Version is 0)
        {
            _logger.LogInformation("Suspending task");
            _completionManager.Complete(string.Empty);

            return Task.FromResult<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>>(
                BackgroundTaskExecutionResult.Suspended.ForEmptyResult().ForEmptyError());
        }

        _completionManager.Complete(string.Empty);
        _logger.LogInformation("Completing task");

        return Task.FromResult<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>>(
            BackgroundTaskExecutionResult.Success.WithEmptyResult().ForEmptyError());
    }
}