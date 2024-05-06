namespace Itmo.Dev.Platform.Enrichment;

public interface IEnrichmentProcessorFactory
{
    IEnrichmentProcessor<TKey, TModel, TState> Create<TKey, TModel, TState>()
        where TKey : notnull
        where TModel : IEnrichedModel<TKey>;
}