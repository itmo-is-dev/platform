using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.Models;

public record StateHandlerContext(BackgroundTaskId BackgroundTaskId, StateTaskMetadata Metadata);