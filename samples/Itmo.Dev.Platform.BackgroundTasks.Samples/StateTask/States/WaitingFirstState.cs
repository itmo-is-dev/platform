namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.States;

public sealed record WaitingFirstState(Guid OperationId) : TaskState;