using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldScheduleAndExecuteTask;

public record TestBackgroundTaskResult(string Value) : IBackgroundTaskResult;