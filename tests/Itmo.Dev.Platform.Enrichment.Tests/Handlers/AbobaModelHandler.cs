using Itmo.Dev.Platform.Common.Models;
using Itmo.Dev.Platform.Enrichment.Tests.Models;

namespace Itmo.Dev.Platform.Enrichment.Tests.Handlers;

public class AbobaModelHandler : IEnrichmentHandler<int, EnrichedModel>
{
    public const string Value = "aboba";
    
    public Task HandleAsync(IEnrichmentContext<int, EnrichedModel, Unit> context, CancellationToken cancellationToken)
    {
        foreach (var model in context.Models)
        {
            model.Value = Value;
        }

        return Task.CompletedTask;
    }
}