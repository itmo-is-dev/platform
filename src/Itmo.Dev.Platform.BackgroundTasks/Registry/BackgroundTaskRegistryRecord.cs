namespace Itmo.Dev.Platform.BackgroundTasks.Registry;

public record BackgroundTaskRegistryRecord(
    string Name,
    Type TaskType,
    Type MetadataType,
    Type ResultType,
    Type ErrorType);