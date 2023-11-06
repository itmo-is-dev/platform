# Itmo.Dev.Platform.BackgroundTasks

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