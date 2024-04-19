using FluentAssertions;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.ProceedAsync_ShouldProceedTaskExecution;
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
using Xunit;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests;

#pragma warning disable CA1506

[Collection(nameof(BackgroundTasksCollectionFixture))]
public class BackgroundTaskProceedTests : TestBase
{
    private readonly BackgroundTasksDatabaseFixture _fixture;

    public BackgroundTaskProceedTests(BackgroundTasksDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProceedAsync_ShouldProceedTaskExecution()
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
                    .AddBackgroundTask(task => task
                        .WithMetadata<EmptyMetadata>()
                        .WithExecutionMetadata<EmptyExecutionMetadata>()
                        .WithResult<EmptyExecutionResult>()
                        .WithError<EmptyError>()
                        .HandleBy<ProceedableBackgroundTask>()));

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
        var manager = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRunner>();

        // Act
        var backgroundTaskId = await manager
            .StartBackgroundTask
            .WithMetadata(EmptyMetadata.Value)
            .WithExecutionMetadata(EmptyExecutionMetadata.Value)
            .RunWithAsync<ProceedableBackgroundTask>(default);

        var timeout = Task.Delay(TimeSpan.FromSeconds(30));
        await Task.WhenAny(timeout, completionManager.WaitTask);

        await manager
            .ProceedBackgroundTask
            .WithId(backgroundTaskId)
            .WithoutExecutionMetadataModification()
            .ProceedAsync(default);

        completionManager.Reset();

        timeout = Task.Delay(TimeSpan.FromSeconds(30));
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
    }
}