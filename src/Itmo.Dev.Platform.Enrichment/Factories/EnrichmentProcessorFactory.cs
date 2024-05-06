using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Enrichment.Factories;

internal class EnrichmentProcessorFactory : IEnrichmentProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EnrichmentProcessorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnrichmentProcessor<TKey, TModel, TState> Create<TKey, TModel, TState>()
        where TKey : notnull
        where TModel : IEnrichedModel<TKey>
    {
        return _serviceProvider.GetRequiredService<IEnrichmentProcessor<TKey, TModel, TState>>();
    }
}