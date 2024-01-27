using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask;

public record StateTaskError(string Message) : IBackgroundTaskError;