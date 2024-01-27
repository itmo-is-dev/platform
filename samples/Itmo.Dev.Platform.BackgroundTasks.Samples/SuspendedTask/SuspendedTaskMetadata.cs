using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.SuspendedTask;

public record SuspendedTaskMetadata(Guid CounterId) : IBackgroundTaskMetadata;