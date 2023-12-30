using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.Common.Models;
using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Platform.BackgroundTasks.Models;

[GenerateBuilder]
public partial record BackgroundTaskQuery(
    BackgroundTaskId[] Ids,
    string[] Names,
    BackgroundTaskState[] States,
    IBackgroundTaskMetadata[] Metadatas,
    IBackgroundTaskExecutionMetadata[] ExecutionMetadatas,
    DateTimeOffset Cursor,
    int? PageSize,
    OrderDirection? OrderDirection);