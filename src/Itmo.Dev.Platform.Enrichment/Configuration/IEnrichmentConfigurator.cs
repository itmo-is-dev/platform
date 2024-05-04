namespace Itmo.Dev.Platform.Enrichment.Configuration;

public interface IEnrichmentConfigurator
{
    IEnrichmentConfigurator Enrich<TKey, TModel>(
        Action<IEnrichmentTypeConfigurator<TKey, TModel>>? configuration = null)
        where TKey : notnull
        where TModel : IEnrichedModel<TKey>;
}

public interface IEnrichmentTypeConfigurator<TKey, out TModel>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    IEnrichmentTypeConfigurator<TKey, TModel> WithHandler<THandler>()
        where THandler : class, IEnrichmentHandler<TKey, TModel>;
}