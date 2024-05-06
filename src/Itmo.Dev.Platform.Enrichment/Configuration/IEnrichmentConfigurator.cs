namespace Itmo.Dev.Platform.Enrichment.Configuration;

public interface IEnrichmentConfigurator
{
    IEnrichmentConfigurator Enrich<TKey, TModel, TState>(
        Action<IEnrichmentTypeConfigurator<TKey, TModel, TState>>? configuration = null)
        where TKey : notnull
        where TModel : IEnrichedModel<TKey>;
}

public interface IEnrichmentTypeConfigurator<TKey, out TModel, TState>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    IEnrichmentTypeConfigurator<TKey, TModel, TState> WithHandler<THandler>()
        where THandler : class, IEnrichmentHandler<TKey, TModel, TState>;
}