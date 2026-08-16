using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges;

public sealed record ValueMetadata(string Value) : IBackgroundTaskMetadata;
