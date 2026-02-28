namespace Itmo.Dev.Platform.Testing.Behavioural.Steps;

public interface IWhenStep<in TFixture> : IFeatureStep<TFixture>
    where TFixture : ITestContext;
