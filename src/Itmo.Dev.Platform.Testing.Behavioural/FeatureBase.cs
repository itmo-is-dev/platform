using Itmo.Dev.Platform.Testing.Behavioural.Steps;
using Xunit;

namespace Itmo.Dev.Platform.Testing.Behavioural;

public abstract class FeatureBase<TFixture> : IAsyncLifetime
    where TFixture : ITestContext
{
    internal IScenarioRunner<TFixture> Runner { get; set; } = null!;

    async Task IAsyncLifetime.InitializeAsync()
    {
        ArgumentNullException.ThrowIfNull(Runner);
        await Runner.InitializeAsync();
        await InitializeAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        ArgumentNullException.ThrowIfNull(Runner);
        await Runner.DisposeAsync();
        await DisposeAsync();
    }

    protected async Task Given(IGivenStep<TFixture> step)
    {
        ArgumentNullException.ThrowIfNull(Runner);
        await Runner.ExecuteStep(step);
    }

    protected async Task When(IWhenStep<TFixture> step)
    {
        ArgumentNullException.ThrowIfNull(Runner);
        await Runner.ExecuteStep(step);
    }

    protected async Task Then(IThenStep<TFixture> step)
    {
        ArgumentNullException.ThrowIfNull(Runner);
        await Runner.ExecuteStep(step);
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task DisposeAsync() => Task.CompletedTask;
}
