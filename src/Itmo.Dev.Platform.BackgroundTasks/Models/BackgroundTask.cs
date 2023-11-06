using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Models;

public record BackgroundTask(
    BackgroundTaskId Id,
    string Name,
    Type Type,
    DateTimeOffset CreatedAt,
    BackgroundTaskState State,
    int RetryNumber,
    IBackgroundTaskMetadata Metadata,
    IBackgroundTaskResult? Result,
    IBackgroundTaskError? Error);