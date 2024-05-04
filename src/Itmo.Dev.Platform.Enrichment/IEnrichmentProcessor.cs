namespace Itmo.Dev.Platform.Enrichment;

public interface IEnrichmentProcessor<TKey, TModel>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    IAsyncEnumerable<TModel> EnrichAsync(IEnumerable<TModel> models, CancellationToken cancellationToken);
}