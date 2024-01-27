using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.SimpleTask;

public sealed record SimpleTaskResult(string Result) : IBackgroundTaskResult;