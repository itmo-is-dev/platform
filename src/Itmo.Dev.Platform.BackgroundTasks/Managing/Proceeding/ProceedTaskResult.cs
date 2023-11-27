using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing.Proceeding;

public abstract record ProceedTaskResult
{
    private ProceedTaskResult() { }

    public sealed record Success(BackgroundTask Task) : ProceedTaskResult;

    public sealed record TaskNotFound : ProceedTaskResult;

    public sealed record MultipleTasksFound(IReadOnlyList<BackgroundTask> Tasks) : ProceedTaskResult;

    public sealed record InvalidTask(BackgroundTask Task) : ProceedTaskResult;

    public sealed record ExecutionMetadataModificationFailure(string Message) : ProceedTaskResult;
}