using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Itmo.Dev.Platform.Testing.Behavioural;

public interface ITestContext : IAsyncLifetime
{
    DateTimeOffset ScenarioStartTimestamp { get; }

    Task IAsyncLifetime.InitializeAsync() => StartAsync();

    Task IAsyncLifetime.DisposeAsync() => StopAsync();

    Task StartAsync();

    Task StopAsync();

    Task OnScenarioStartingAsync();

    Task OnScenarioFinishedAsync();

    T GetRequiredService<T>()
        where T : class;

    AsyncServiceScope CreateScope();
}
