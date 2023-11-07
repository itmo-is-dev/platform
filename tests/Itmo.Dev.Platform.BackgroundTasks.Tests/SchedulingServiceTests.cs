using FluentAssertions;
using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Persistence.Plugins;
using Itmo.Dev.Platform.BackgroundTasks.Persistence.Repositories;
using Itmo.Dev.Platform.BackgroundTasks.Registry;
using Itmo.Dev.Platform.BackgroundTasks.Scheduling;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Fixtures;
using Itmo.Dev.Platform.Postgres.Plugins;
using Itmo.Dev.Platform.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests;

public class SchedulingServiceTests : TestBase, IClassFixture<SchedulingServiceTests.Fixture>
{
    private readonly Fixture _fixture;

    public SchedulingServiceTests(Fixture fixture, ITestOutputHelper output) : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotResultInPreparedTransactionException()
    {
        // Arrange
        var optionsMock = new Mock<IOptionsMonitor<BackgroundTaskSchedulingOptions>>();

        optionsMock
            .Setup(x => x.CurrentValue)
            .Returns(new BackgroundTaskSchedulingOptions
            {
                BatchSize = 10,
                PollingDelay = TimeSpan.FromMilliseconds(500),
            });

        await using var scope = _fixture.Scope;

        await scope.UsePlatformBackgroundTasksAsync(default);

        var logger = new TestLogger<BackgroundTaskSchedulingService>(
            scope.ServiceProvider.GetRequiredService<ILogger<BackgroundTaskSchedulingService>>());

        var service = new BackgroundTaskSchedulingService(
            optionsMock.Object,
            scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>(),
            logger);

        // Act
        await service.StartAsync(default);
        await Task.Delay(TimeSpan.FromSeconds(2));

        await service.StopAsync(default);
        
        // Assert
        logger.ErrorCount.Should().Be(0);
    }

    public class Fixture : BackgroundTasksDatabaseFixture
    {
        protected override void ConfigureServices(IServiceCollection collection)
        {
            base.ConfigureServices(collection);

            collection
                .AddOptions<BackgroundTaskPersistenceOptions>()
                .Configure(c => c.SchemaName = "background_tasks");

            collection
                .AddSingleton<IDataSourcePlugin, BackgroundTaskDataSourcePlugin>()
                .AddSingleton<IBackgroundTaskRegistry, BackgroundTaskRegistry>()
                .AddSingleton(new JsonSerializerSettings())
                .AddSingleton<BackgroundTaskRepositoryQueryStorage>()
                .AddScoped<BackgroundTaskRepository>()
                .AddScoped<IBackgroundTaskInfrastructureRepository>(
                    x => x.GetRequiredService<BackgroundTaskRepository>())
                .AddScoped<IBackgroundTaskRepository>(x => x.GetRequiredService<BackgroundTaskRepository>())
                .AddScoped<IBackgroundTaskScheduler>(_ => Mock.Of<IBackgroundTaskScheduler>());
        }
    }

    private class TestLogger<T> : ILogger<T>
    {
        private readonly ILogger<T> _logger;

        public TestLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public int ErrorCount { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel is LogLevel.Error)
                ErrorCount++;

            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}