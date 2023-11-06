using FluentAssertions;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldScheduleAndExecuteTask;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldSetStateFailedWhenRetryCountExceeded;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Fixtures;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.Models;
using Itmo.Dev.Platform.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;
using BackgroundTaskQuery = Itmo.Dev.Platform.BackgroundTasks.Models.BackgroundTaskQuery;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests;

#pragma warning disable CA1506

[Collection(nameof(BackgroundTasksCollectionFixture))]
public class BackgroundTaskTests : TestBase
{
    private readonly BackgroundTasksDatabaseFixture _backgroundTasksFixture;
    private readonly CompletionManager _completionManager;

    public BackgroundTaskTests(BackgroundTasksDatabaseFixture backgroundTasksFixture, ITestOutputHelper output)
        : base(output, LogEventLevel.Warning)
    {
        _backgroundTasksFixture = backgroundTasksFixture;
        _completionManager = new CompletionManager();
    }

    [Fact]
    public async Task RunWithAsync_ShouldScheduleAndExecuteTask()
    {
        // Arrange
        await using var fixtureScope = _backgroundTasksFixture.Scope;

        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureServices(collection =>
            {
                var configuration = new ConfigurationManager();

                collection.AddPlatformBackgroundTasks(backgroundTaskBuilder => backgroundTaskBuilder
                    .ConfigurePersistence(configuration, options => options.SchemaName = "background_tasks")
                    .ConfigureScheduling(configuration,
                        options =>
                        {
                            options.BatchSize = 100;
                            options.PollingDelay = TimeSpan.FromMilliseconds(500);
                        })
                    .ConfigureExecution(configuration, options => options.MaxRetryCount = 10)
                    .AddBackgroundTask(task => task
                        .WithMetadata<TestBackgroundTaskMetadata>()
                        .WithResult<TestBackgroundTaskResult>()
                        .WithError<EmptyError>()
                        .HandleBy<TestBackgroundTask>()));

                // ReSharper disable once AccessToDisposedClosure
                var connectionString = fixtureScope.ServiceProvider.GetRequiredService<PostgresConnectionString>();

                collection.AddPlatformPostgres(_ => { });
                collection.RemoveAll<PostgresConnectionString>();
                collection.AddSingleton(connectionString);

                collection.AddSingleton(_completionManager);

                collection.AddLogging(b => b.AddSerilog());
            }));

        application.CreateClient();

        await using var scope = application.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRunner>();

        var metadata = new TestBackgroundTaskMetadata("aboba");

        // Act
        var backgroundTaskId = await manager
            .StartBackgroundTask
            .WithMetadata(metadata)
            .RunWithAsync<TestBackgroundTask>(default);

        var timeout = Task.Delay(TimeSpan.FromSeconds(30));
        await Task.WhenAny(timeout, _completionManager.Task);

        // Assert
        _completionManager.Task.IsCompletedSuccessfully.Should().BeTrue();
        _completionManager.Task.Result.Should().Be(metadata.Value);

        var repository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();

        var backgroundTask = await repository
            .QueryAsync(
                BackgroundTaskQuery.Build(x => x.WithId(backgroundTaskId)),
                default)
            .SingleAsync();

        backgroundTask.State.Should().Be(BackgroundTaskState.Completed);
        backgroundTask.Result.Should().BeOfType<TestBackgroundTaskResult>().Which.Value.Should().Be(metadata.Value);
    }

    [Fact]
    public async Task RunWithAsync_ShouldSetStateFailedWhenRetryCountExceeded()
    {
        // Arrange
        await using var fixtureScope = _backgroundTasksFixture.Scope;

        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureServices(collection =>
            {
                var configuration = new ConfigurationManager();

                collection.AddPlatformBackgroundTasks(backgroundTaskBuilder => backgroundTaskBuilder
                    .ConfigurePersistence(configuration, options => options.SchemaName = "background_tasks")
                    .ConfigureScheduling(configuration,
                        options =>
                        {
                            options.BatchSize = 100;
                            options.PollingDelay = TimeSpan.FromMilliseconds(500);
                            options.SchedulerRetryCount = 0;
                        })
                    .ConfigureExecution(configuration, options => options.MaxRetryCount = 2)
                    .AddBackgroundTask(task => task
                        .WithMetadata<EmptyMetadata>()
                        .WithResult<EmptyExecutionResult>()
                        .WithError<EmptyError>()
                        .HandleBy<FailingBackgroundTask>()));

                // ReSharper disable once AccessToDisposedClosure
                var connectionString = fixtureScope.ServiceProvider.GetRequiredService<PostgresConnectionString>();

                collection.AddPlatformPostgres(_ => { });
                collection.RemoveAll<PostgresConnectionString>();
                collection.AddSingleton(connectionString);

                collection.AddSingleton(_completionManager);

                collection.AddLogging(b => b.AddSerilog());
            }));

        application.CreateClient();

        await using var scope = application.Services.CreateAsyncScope();

        var manager = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRunner>();
        var repository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();

        var metadata = EmptyMetadata.Value;

        // Act
        var backgroundTaskId = await manager
            .StartBackgroundTask
            .WithMetadata(metadata)
            .RunWithAsync<FailingBackgroundTask>(default);

        var timeout = Task.Delay(TimeSpan.FromSeconds(10));

        var checker = Task.Run(async () =>
        {
            BackgroundTask task;

            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                task = await GetBackgroundTask(backgroundTaskId, repository);
            }
            while (task.State is not BackgroundTaskState.Failed);

            return task;
        });

        await Task.WhenAny(checker, timeout);
        
        // Assert
        checker.IsCompletedSuccessfully.Should().BeTrue();
        checker.Result.State.Should().Be(BackgroundTaskState.Failed);
    }

    private static async Task<BackgroundTask> GetBackgroundTask(
        BackgroundTaskId id,
        IBackgroundTaskRepository repository)
    {
        var query = BackgroundTaskQuery.Build(x => x.WithId(id));
        return await repository.QueryAsync(query, default).SingleAsync();
    }
}