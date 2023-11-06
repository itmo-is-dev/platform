namespace Itmo.Dev.Platform.BackgroundTasks.Models;

public enum BackgroundTaskState
{
    Pending,
    Enqueued,
    Retrying,
    Failed,
    Cancelled,
    Completed,
}