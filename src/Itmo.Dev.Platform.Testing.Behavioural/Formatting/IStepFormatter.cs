using Itmo.Dev.Platform.Testing.Behavioural.Steps;
using Itmo.Dev.Platform.Testing.Behavioural.Text;

namespace Itmo.Dev.Platform.Testing.Behavioural.Formatting;

public interface IStepFormatter<TContext>
    where TContext : ITestContext
{
    void Format(IFeatureStep<TContext> step, ITextWriter writer);
}
