using Itmo.Dev.Platform.Common.Models;

namespace Itmo.Dev.Platform.Enrichment.Extensions;

public static class EnrichmentProcessorExtensions
{
    public static IAsyncEnumerable<TModel> EnrichAsync<TKey, TModel>(
        this IEnrichmentProcessor<TKey, TModel, Unit> processor,
        IEnumerable<TModel> models,
        CancellationToken cancellationToken)
        where TKey : notnull
        where TModel : IEnrichedModel<TKey>
    {
        return processor.EnrichAsync(Unit.Value, models, cancellationToken);
    }
}