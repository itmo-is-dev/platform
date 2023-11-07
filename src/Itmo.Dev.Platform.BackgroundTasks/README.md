# Itmo.Dev.Platform.BackgroundTasks

- [Execution metadata](#executionmetadata)
- [Configuration](#configuration)

## ExecutionMetadata

Execution metadata used for restoring task's execution progress from the point it was suspended.

Suspension can occur when task execution is cancelled due to application shutdown
(when OperationCancelledException or TaskCancelledException is thrown).

In this case, execution metadata is persisted and would be loaded when task execution proceeds (when application is restarted).

You can use mutable model as execution metadata, modifying it as the execution process goes. \
To avoid repeating operation execution (ensuring idempotency) modify execution metadata only when changes are persisted,
ex: transaction is committed and you update page token of some long running operataion. 

## Configuration

```csharp
collection.AddPlatformBackgroundTasks(backgroundTaskBuilder => backgroundTaskBuilder
    .ConfigurePersistence(persistenceConfiguration)
    .ConfigureScheduling(schedulingConfiguration)
    .ConfigureExecution(executionConfiguration)
    .AddBackgroundTask(task => task
        .WithMetadata<TestBackgroundTaskMetadata>()
        .WithResult<TestBackgroundTaskResult>()
        .WithError<EmptyError>()
        .HandleBy<TestBackgroundTask>()));
```

### Schema

#### Persistence configuration

```json
{
  "SchemaName": string
}
```

- SchemaName \
  Name of a PostgreSQL schema to store background task data

#### Scheduling configuration

```json
{
  "BatchSize": int,
  "PollingDelay": timespan,
  "SchedulerRetryCount": int,
  "SchedulerRetryDelays": [int]
}
```

- BatchSize \
  Number of tasks fetched per enqueuing run
- PollingDelay \
  Delay between enqueuing runs
- SchedulerRetryCount \
  Count of retries that hangfire will do, before marking task failed
- SchedulerRetryDelays \
  Delay between hangfire retries, index corresponds to retry number


#### Execution configuration

```json
{
  "MaxRetryCount": int
}
```

- MaxRetryCount \
  Count of enqueueing retries for before task is moved into failed state