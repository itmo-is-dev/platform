namespace Itmo.Dev.Platform.Enrichment;

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

    IEnrichmentTypeConfigurator<TKey, TModel, TState> WithTransitive<TTransitiveKey, TTransitiveModel>(
        Func<TModel, TTransitiveModel> func)
        where TTransitiveKey : notnull
        where TTransitiveModel : IEnrichedModel<TTransitiveKey>;

    IEnrichmentTypeConfigurator<TKey, TModel, TState> WithNullableTransitive<TTransitiveKey, TTransitiveModel>(
        Func<TModel, TTransitiveModel?> func)
        where TTransitiveKey : notnull
        where TTransitiveModel : IEnrichedModel<TTransitiveKey>;
}
