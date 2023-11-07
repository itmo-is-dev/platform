namespace Itmo.Dev.Platform.BackgroundTasks.Registry;

public record BackgroundTaskRegistryRecord(
    string Name,
    Type TaskType,
    Type MetadataType,
    Type ExecutionMetadataType,
    Type ResultType,
    Type ErrorType);