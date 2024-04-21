using FluentAssertions;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Managing.Proceeding;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteSimpleStateMachine;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteSimpleStateMachine.States;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends.
    States;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldScheduleAndExecuteTask;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Fixtures;
using Itmo.Dev.Platform.Common.Lifetime;
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

namespace Itmo.Dev.Platform.BackgroundTasks.Tests;

#pragma warning disable CA1506

[Collection(nameof(BackgroundTasksCollectionFixture))]
public class BackgroundTaskStateTests : TestBase
{
    private readonly BackgroundTasksDatabaseFixture _fixture;

    public BackgroundTaskStateTests(BackgroundTasksDatabaseFixture fixture, ITestOutputHelper output)
        : base(output, LogEventLevel.Warning)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RunWithAsync_ShouldExecuteSimpleStateMachine()
    {
        // Arrange
        await using var fixtureScope = _fixture.Scope;

        var completionManager = new CompletionManager();

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
                    .AddStateMachine()
                    .AddBackgroundTask(task => task
                        .WithMetadata<EmptyMetadata>()
                        .WithExecutionMetadata<SimpleStateExecutionMetadata>()
                        .WithResult<EmptyExecutionResult>()
                        .WithError<EmptyError>()
                        .HandleBy<SimpleStateTask>()));

                // ReSharper disable once AccessToDisposedClosure
                var connectionString = fixtureScope.ServiceProvider.GetRequiredService<PostgresConnectionString>();

                collection.AddPlatformPostgres(_ => { });
                collection.RemoveAll<PostgresConnectionString>();
                collection.AddSingleton(connectionString);

                collection.AddSingleton(completionManager);

                collection.AddLogging(b => b.AddSerilog());
            }));

        application.CreateClient();

        var platformLifetime = application.Services.GetRequiredService<IPlatformLifetime>();
        await platformLifetime.WaitOnInitializedAsync(default);

        await using var scope = application.Services.CreateAsyncScope();
        var runner = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRunner>();

        // Act
        var backgroundTaskId = await runner.StartBackgroundTask
            .WithMetadata(EmptyMetadata.Value)
            .WithExecutionMetadata(new SimpleStateExecutionMetadata())
            .RunWithAsync<SimpleStateTask>(default);

        var timeout = Task.Delay(TimeSpan.FromSeconds(30));
        await Task.WhenAny(timeout, completionManager.WaitTask);

        // Assert
        completionManager.WaitTask.IsCompletedSuccessfully.Should().BeTrue();

        var repository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();

        var backgroundTask = await repository
            .QueryAsync(
                BackgroundTaskQuery.Build(x => x.WithId(backgroundTaskId)),
                default)
            .SingleAsync();

        backgroundTask.State.Should().Be(BackgroundTaskState.Completed);

        backgroundTask
            .ExecutionMetadata.Should()
            .BeOfType<SimpleStateExecutionMetadata>()
            .Which.State.Should()
            .BeOfType<CompletedSimpleState>();
    }

    [Fact]
    public async Task RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends()
    {
        // Arrange
        await using var fixtureScope = _fixture.Scope;

        var completionManager = new CompletionManager();

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
                    .AddStateMachine()
                    .AddBackgroundTask(task => task
                        .WithMetadata<EmptyMetadata>()
                        .WithExecutionMetadata<SuspendedTaskExecutionMetadata>()
                        .WithResult<EmptyExecutionResult>()
                        .WithError<EmptyError>()
                        .HandleBy<SuspendedStateTask>()));

                // ReSharper disable once AccessToDisposedClosure
                var connectionString = fixtureScope.ServiceProvider.GetRequiredService<PostgresConnectionString>();

                collection.AddPlatformPostgres(_ => { });
                collection.RemoveAll<PostgresConnectionString>();
                collection.AddSingleton(connectionString);

                collection.AddSingleton(completionManager);

                collection.AddLogging(b => b.AddSerilog());
            }));

        application.CreateClient();

        var platformLifetime = application.Services.GetRequiredService<IPlatformLifetime>();
        await platformLifetime.WaitOnInitializedAsync(default);

        await using var scope = application.Services.CreateAsyncScope();
        var runner = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRunner>();

        // Act
        var backgroundTaskId = await runner.StartBackgroundTask
            .WithMetadata(EmptyMetadata.Value)
            .WithExecutionMetadata(new SuspendedTaskExecutionMetadata())
            .RunWithAsync<SuspendedStateTask>(default);

        var timeout = Task.Delay(TimeSpan.FromSeconds(5));
        await Task.WhenAny(timeout, completionManager.WaitTask);

        completionManager.WaitTask.IsCompletedSuccessfully.Should().BeTrue();

        await Task.Delay(TimeSpan.FromMilliseconds(1000));
        completionManager.Reset();

        var proceedResult = await runner.ProceedBackgroundTask
            .WithId(backgroundTaskId)
            .WithExecutionMetadataModification<SuspendedTaskExecutionMetadata>(
                metadata => metadata.State = new ProceededSuspendedTaskState())
            .ProceedAsync(default);

        proceedResult.Should().BeOfType<ProceedTaskResult.Success>();

        timeout = Task.Delay(TimeSpan.FromSeconds(5));
        await Task.WhenAny(timeout, completionManager.WaitTask);

        // Assert
        completionManager.WaitTask.IsCompletedSuccessfully.Should().BeTrue();

        var repository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();

        var backgroundTask = await repository
            .QueryAsync(
                BackgroundTaskQuery.Build(x => x.WithId(backgroundTaskId)),
                default)
            .SingleAsync();

        backgroundTask.State.Should().Be(BackgroundTaskState.Completed);

        backgroundTask
            .ExecutionMetadata.Should()
            .BeOfType<SuspendedTaskExecutionMetadata>()
            .Which.State.Should()
            .BeOfType<CompletedSuspendedTaskState>();
    }
}