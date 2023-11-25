using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldScheduleAndExecuteTask;

public record TestBackgroundTaskMetadata(string Value) : IBackgroundTaskMetadata;