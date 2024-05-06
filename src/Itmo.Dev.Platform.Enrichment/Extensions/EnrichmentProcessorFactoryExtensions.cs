using Itmo.Dev.Platform.Common.Models;

namespace Itmo.Dev.Platform.Enrichment.Extensions;

public static class EnrichmentProcessorFactoryExtensions
{
    public static IEnrichmentProcessor<TKey, TModel, Unit> Create<TKey, TModel>(
        this IEnrichmentProcessorFactory factory)
        where TKey : notnull
        where TModel : IEnrichedModel<TKey>
    {
        return factory.Create<TKey, TModel, Unit>();
    }
}