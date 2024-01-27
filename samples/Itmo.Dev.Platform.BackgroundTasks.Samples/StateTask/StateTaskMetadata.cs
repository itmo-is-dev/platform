using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask;

public sealed record StateTaskMetadata(Guid OperationId) : IBackgroundTaskMetadata;