namespace Itmo.Dev.Platform.Testing.Behavioural.Steps;

public interface IThenStep<in TFixture> : IFeatureStep<TFixture>
    where TFixture : ITestContext;
