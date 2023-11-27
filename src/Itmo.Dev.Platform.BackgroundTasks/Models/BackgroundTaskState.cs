namespace Itmo.Dev.Platform.BackgroundTasks.Models;

public enum BackgroundTaskState
{
    Pending = 0,
    Enqueued = 1,
    Retrying = 2,
    Failed = 3,
    Cancelled = 4,
    Completed = 5,
    Proceeded = 6,
    Suspended = 7,
}