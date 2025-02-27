using FluentAssertions;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Postgres.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.ScheduleWithAsync_ShouldScheduleTask_OnlyWhenScheduleTimeArrives;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.SuspendedUntil_ShouldProceedOnItsOwn_WhenScheduleTimeArrives;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Fixtures;
using Itmo.Dev.Platform.Common.DateTime;
using Itmo.Dev.Platform.Common.Lifetime;
using Itmo.Dev.Platform.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests;

#pragma warning disable CA1506

[Collection(nameof(BackgroundTasksCollectionFixture))]
public class BackgroundTasksScheduledAtTests : TestBase
{
    private readonly BackgroundTasksDatabaseFixture _backgroundTasksFixture;

    public BackgroundTasksScheduledAtTests(
        BackgroundTasksDatabaseFixture backgroundTasksFixture,
        ITestOutputHelper output)
        : base(output, LogEventLevel.Warning)
    {
        _backgroundTasksFixture = backgroundTasksFixture;
    }

    [Fact]
    public async Task ScheduleWithAsync_ShouldScheduleTask_OnlyWhenScheduleTimeArrives()
    {
        // Arrange
        await using var fixtureScope = _backgroundTasksFixture.Scope;

        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(
                builder => builder.ConfigureServices(
                    collection =>
                    {
                        var configuration = new ConfigurationManager();

                        collection.AddPlatformBackgroundTasks(
                            backgroundTaskBuilder => backgroundTaskBuilder
                                .UsePostgresPersistence(
                                    b => b.Configure(o => o.SchemaName = "background_tasks"))
                                .ConfigureScheduling(
                                    b => b.Configure(
                                        options =>
                                        {
                                            options.BatchSize = 100;
                                            options.PollingDelay = TimeSpan.FromMilliseconds(500);
                                        }))
                                .UseHangfireScheduling(
                                    x => x
                                        .ConfigureOptions(_ => { })
                                        .UsePostgresJobStorage())
                                .ConfigureExecution(configuration, options => options.MaxRetryCount = 10)
                                .AddBackgroundTask(
                                    task => task
                                        .WithMetadata<ScheduledAtMetadata>()
                                        .WithExecutionMetadata<EmptyExecutionMetadata>()
                                        .WithResult<EmptyExecutionResult>()
                                        .WithError<EmptyError>()
                                        .HandleBy<ScheduledAtBackgroundTask>()));

                        _backgroundTasksFixture.AddPlatformPersistence(collection);
                        collection.AddLogging(b => b.AddSerilog());
                    }));

        application.CreateClient();

        var platformLifetime = application.Services.GetRequiredService<IPlatformLifetime>();
        await platformLifetime.WaitOnInitializedAsync(default);

        await using var scope = application.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRunner>();

        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var delay = TimeSpan.FromSeconds(60);
        var scheduledAt = dateTimeProvider.Current.Add(delay);

        var metadata = new ScheduledAtMetadata(scheduledAt);

        // Act
        var backgroundTaskId = await manager
            .StartBackgroundTask
            .WithMetadata(metadata)
            .WithExecutionMetadata(EmptyExecutionMetadata.Value)
            .ScheduleWithAsync<ScheduledAtBackgroundTask>(scheduledAt, default);

        await Task.Delay(delay);

        // Assert
        using var cts = new CancellationTokenSource(delay);

        while (cts.IsCancellationRequested is false)
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();

            var backgroundTask = await repository
                .QueryAsync(
                    BackgroundTaskQuery.Build(x => x.WithId(backgroundTaskId)),
                    cts.Token)
                .SingleAsync(cts.Token);

            if (backgroundTask.State is not BackgroundTaskState.Completed and not BackgroundTaskState.Failed)
                continue;

            backgroundTask.State.Should().Be(BackgroundTaskState.Completed);
            return;
        }

        Assert.Fail();
    }

    [Fact]
    public async Task SuspendedUntil_ShouldProceedOnItsOwn_WhenScheduleTimeArrives()
    {
                // Arrange
        await using var fixtureScope = _backgroundTasksFixture.Scope;

        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(
                builder => builder.ConfigureServices(
                    collection =>
                    {
                        var configuration = new ConfigurationManager();

                        collection.AddPlatformBackgroundTasks(
                            backgroundTaskBuilder => backgroundTaskBuilder
                                .UsePostgresPersistence(
                                    b => b.Configure(o => o.SchemaName = "background_tasks"))
                                .ConfigureScheduling(
                                    b => b.Configure(
                                        options =>
                                        {
                                            options.BatchSize = 100;
                                            options.PollingDelay = TimeSpan.FromMilliseconds(500);
                                        }))
                                .UseHangfireScheduling(
                                    x => x
                                        .ConfigureOptions(_ => { })
                                        .UsePostgresJobStorage())
                                .ConfigureExecution(configuration, options => options.MaxRetryCount = 10)
                                .AddBackgroundTask(
                                    task => task
                                        .WithMetadata<SuspendedUntilMetadata>()
                                        .WithExecutionMetadata<SuspendedUntilExecutionMetadata>()
                                        .WithResult<EmptyExecutionResult>()
                                        .WithError<EmptyError>()
                                        .HandleBy<SuspendedUntilBackgroundTask>()));

                        _backgroundTasksFixture.AddPlatformPersistence(collection);
                        collection.AddLogging(b => b.AddSerilog());
                    }));

        application.CreateClient();

        var platformLifetime = application.Services.GetRequiredService<IPlatformLifetime>();
        await platformLifetime.WaitOnInitializedAsync(default);

        await using var scope = application.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRunner>();

        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var delay = TimeSpan.FromSeconds(60);
        var scheduledAt = dateTimeProvider.Current.Add(delay);

        var metadata = new SuspendedUntilMetadata(scheduledAt);

        // Act
        var backgroundTaskId = await manager
            .StartBackgroundTask
            .WithMetadata(metadata)
            .WithExecutionMetadata(new SuspendedUntilExecutionMetadata())
            .RunWithAsync<SuspendedUntilBackgroundTask>(default);

        await Task.Delay(delay);

        // Assert
        using var cts = new CancellationTokenSource(delay);

        while (cts.IsCancellationRequested is false)
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();

            var backgroundTask = await repository
                .QueryAsync(
                    BackgroundTaskQuery.Build(x => x.WithId(backgroundTaskId)),
                    cts.Token)
                .SingleAsync(cts.Token);

            if (backgroundTask.State is not BackgroundTaskState.Completed and not BackgroundTaskState.Failed)
                continue;

            backgroundTask.State.Should().Be(BackgroundTaskState.Completed);
            return;
        }

        Assert.Fail();
    }
}
