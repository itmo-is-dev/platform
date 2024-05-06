using Itmo.Dev.Platform.Common.Models;

namespace Itmo.Dev.Platform.Enrichment;

public interface IEnrichmentHandler<TKey, in TModel, TState>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    Task HandleAsync(IEnrichmentContext<TKey, TModel, TState> context, CancellationToken cancellationToken);
}

public interface IEnrichmentHandler<TKey, in TModel> : IEnrichmentHandler<TKey, TModel, Unit>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>;