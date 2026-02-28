namespace Itmo.Dev.Platform.Testing.Behavioural.Steps;

public interface IGivenStep<in TFixture> : IFeatureStep<TFixture>
    where TFixture : ITestContext;
