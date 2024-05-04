namespace Itmo.Dev.Platform.Enrichment.Contexts;

internal class EnrichmentContext<TKey, TModel> : IEnrichmentContext<TKey, TModel>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    private readonly IReadOnlyDictionary<TKey, TModel> _models;

    public EnrichmentContext(IEnumerable<TModel> models)
    {
        _models = models.ToDictionary(x => x.Key);
    }

    public IEnumerable<TKey> Keys => _models.Keys;

    public IEnumerable<TModel> Models => _models.Values;

    public TModel GetModel(TKey key) => _models[key];
}