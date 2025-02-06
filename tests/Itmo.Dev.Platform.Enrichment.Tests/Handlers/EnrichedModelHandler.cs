using Itmo.Dev.Platform.Common.Models;
using Itmo.Dev.Platform.Enrichment.Tests.Models;

namespace Itmo.Dev.Platform.Enrichment.Tests.Handlers;

public class EnrichedModelHandler : IEnrichmentHandler<int, EnrichedModel>
{
    private readonly string _value;

    public EnrichedModelHandler(string value)
    {
        _value = value;
    }

    public Task HandleAsync(IEnrichmentContext<int, EnrichedModel, Unit> context, CancellationToken cancellationToken)
    {
        foreach (var model in context.Models)
        {
            model.Value = _value;
        }

        return Task.CompletedTask;
    }
}
