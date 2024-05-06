namespace Itmo.Dev.Platform.Enrichment.Contexts;

internal class EnrichmentContext<TKey, TModel, TState> : IEnrichmentContext<TKey, TModel, TState>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    private readonly IReadOnlyDictionary<TKey, TModel> _models;

    public EnrichmentContext(IEnumerable<TModel> models, TState state)
    {
        State = state;
        _models = models.ToDictionary(x => x.Key);
    }

    public IEnumerable<TKey> Keys => _models.Keys;

    public IEnumerable<TModel> Models => _models.Values;

    public TState State { get; }

    public TModel GetModel(TKey key) => _models[key];
}