namespace Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

public class EmptyExecutionResult : IBackgroundTaskResult
{
    private EmptyExecutionResult() { }

    public static readonly EmptyExecutionResult Value = new EmptyExecutionResult();
}