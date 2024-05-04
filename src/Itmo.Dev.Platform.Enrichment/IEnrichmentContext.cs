namespace Itmo.Dev.Platform.Enrichment;

public interface IEnrichmentContext<TKey, out TModel>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    IEnumerable<TKey> Keys { get; }
    
    IEnumerable<TModel> Models { get; }

    TModel GetModel(TKey key);
}