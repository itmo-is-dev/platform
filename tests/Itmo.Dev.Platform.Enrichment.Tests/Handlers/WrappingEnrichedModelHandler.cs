using Itmo.Dev.Platform.Common.Models;
using Itmo.Dev.Platform.Enrichment.Tests.Models;

namespace Itmo.Dev.Platform.Enrichment.Tests.Handlers;

public class WrappingEnrichedModelHandler : IEnrichmentHandler<int, WrappingEnrichedModel>
{
    private readonly string _value;

    public WrappingEnrichedModelHandler(string value)
    {
        _value = value;
    }

    public Task HandleAsync(
        IEnrichmentContext<int, WrappingEnrichedModel, Unit> context,
        CancellationToken cancellationToken)
    {
        foreach (WrappingEnrichedModel model in context.Models)
        {
            model.OuterValue = _value;
        }
        
        return Task.CompletedTask;
    }
}
