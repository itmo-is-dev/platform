using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Testing.Behavioural.Contexts;

public abstract class TestContextBase : ITestContext
{
    public DateTimeOffset ScenarioStartTimestamp { get; private set; }

    public abstract Task StartAsync();

    public abstract Task StopAsync();

    public virtual Task OnScenarioStartingAsync()
    {
        ScenarioStartTimestamp = DateTimeOffset.UtcNow;
        return Task.CompletedTask;
    }

    public abstract Task OnScenarioFinishedAsync();

    public abstract T GetRequiredService<T>()
        where T : class;

    public abstract AsyncServiceScope CreateScope();
}
