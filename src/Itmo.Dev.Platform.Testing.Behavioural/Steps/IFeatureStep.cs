namespace Itmo.Dev.Platform.Testing.Behavioural.Steps;

public interface IFeatureStep<in TContext>
    where TContext : ITestContext
{
    ValueTask ExecuteAsync(TContext context, CancellationToken cancellationToken);
}
