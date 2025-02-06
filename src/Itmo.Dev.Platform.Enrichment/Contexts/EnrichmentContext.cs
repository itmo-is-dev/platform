namespace Itmo.Dev.Platform.Enrichment.Contexts;

internal class EnrichmentContext<TKey, TModel, TState> : IEnrichmentContext<TKey, TModel, TState>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    private readonly ILookup<TKey, TModel> _models;

    public EnrichmentContext(IEnumerable<TModel> models, TState state)
    {
        State = state;
        _models = models.ToLookup(x => x.Key);
    }

    public IEnumerable<TKey> Keys
        => _models.Select(grouping => grouping.Key);

    public IEnumerable<TModel> Models
        => _models.SelectMany(grouping => grouping);

    public TState State { get; }

    public TModel GetModel(TKey key)
        => _models[key].SingleOrDefault() ?? throw PlatformEnrichmentException.ModelNotFound<TKey, TModel>(key);

    public TModel? FindModel(TKey key)
        => _models[key].SingleOrDefault();

    public IEnumerable<TModel> FindModels(TKey key)
        => _models[key];
}
