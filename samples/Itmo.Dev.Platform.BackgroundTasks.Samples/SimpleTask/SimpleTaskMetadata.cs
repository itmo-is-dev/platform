using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.SimpleTask;

public sealed record SimpleTaskMetadata(string[] Values) : IBackgroundTaskMetadata;