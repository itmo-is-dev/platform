using Itmo.Dev.Platform.Testing.Behavioural.Steps;
using Xunit;

namespace Itmo.Dev.Platform.Testing.Behavioural;

public interface IScenarioRunner<TFixture> : IAsyncLifetime
    where TFixture : ITestContext
{
    Task ExecuteStep(IFeatureStep<TFixture> step);
}
