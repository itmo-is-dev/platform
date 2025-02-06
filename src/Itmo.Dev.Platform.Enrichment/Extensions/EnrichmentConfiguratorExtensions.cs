using Itmo.Dev.Platform.Common.Models;

// ReSharper disable once CheckNamespace
namespace Itmo.Dev.Platform.Enrichment;

public static class EnrichmentConfiguratorExtensions
{
    public static IEnrichmentConfigurator Enrich<TKey, TModel>(
        this IEnrichmentConfigurator configurator,
        Action<IEnrichmentTypeConfigurator<TKey, TModel, Unit>>? configuration = null)
        where TKey : notnull
        where TModel : IEnrichedModel<TKey>
    {
        return configurator.Enrich(configuration);
    }
}
