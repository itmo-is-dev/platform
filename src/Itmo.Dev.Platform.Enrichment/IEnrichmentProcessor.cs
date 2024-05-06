namespace Itmo.Dev.Platform.Enrichment;

public interface IEnrichmentProcessor<TKey, TModel, in TState>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    IAsyncEnumerable<TModel> EnrichAsync(TState state, IEnumerable<TModel> models, CancellationToken cancellationToken);
}