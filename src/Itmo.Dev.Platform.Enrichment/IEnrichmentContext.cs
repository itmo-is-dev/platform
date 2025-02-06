namespace Itmo.Dev.Platform.Enrichment;

public interface IEnrichmentContext<TKey, out TModel, TState>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    IEnumerable<TKey> Keys { get; }

    IEnumerable<TModel> Models { get; }

    TState State { get; }

    TModel GetModel(TKey key);

    TModel? FindModel(TKey key);

    IEnumerable<TModel> FindModels(TKey key);
}
