namespace Itmo.Dev.Platform.Enrichment;

public interface IEnrichmentHandler<TKey, in TModel>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    Task HandleAsync(IEnrichmentContext<TKey, TModel> context, CancellationToken cancellationToken);
}