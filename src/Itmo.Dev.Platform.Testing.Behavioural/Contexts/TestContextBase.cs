using Itmo.Dev.Platform.Testing.Behavioural.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.Testing.Behavioural.Contexts;

public abstract class TestContextBase : ITestContext
{
    private readonly DynamicXunitSink _logEventSink = new();

    public DateTimeOffset ScenarioStartTimestamp { get; private set; }

    protected ILogEventSink TestOutputSink => _logEventSink;

    public abstract Task StartAsync();

    public abstract Task StopAsync();

    async Task ITestContext.OnScenarioStartingAsync()
    {
        ScenarioStartTimestamp = DateTimeOffset.UtcNow;
        await OnScenarioStartingAsync();
    }

    async Task ITestContext.OnScenarioFinishedAsync()
    {
        await OnScenarioFinishedAsync();
        _logEventSink.ClearTestOutput();
    }

    public void UseOutput(ITestOutputHelper output)
    {
        _logEventSink.SetTestOutput(output);
    }

    public abstract T GetRequiredService<T>()
        where T : class;

    public abstract AsyncServiceScope CreateScope();

    protected virtual Task OnScenarioStartingAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnScenarioFinishedAsync()
    {
        return Task.CompletedTask;
    }
}
