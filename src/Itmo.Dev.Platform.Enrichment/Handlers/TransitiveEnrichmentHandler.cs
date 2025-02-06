namespace Itmo.Dev.Platform.Enrichment.Handlers;

internal class TransitiveEnrichmentHandler<TKey, TModel, TTransitiveKey, TTransitiveModel, TState>
    : IEnrichmentHandler<TKey, TModel, TState>
    where TKey : notnull
    where TTransitiveKey : notnull
    where TModel : IEnrichedModel<TKey>
    where TTransitiveModel : IEnrichedModel<TTransitiveKey>
{
    private readonly IEnrichmentProcessorFactory _enrichmentFactory;
    private readonly Func<TModel, TTransitiveModel> _extractor;

    public TransitiveEnrichmentHandler(
        IEnrichmentProcessorFactory enrichmentFactory,
        Func<TModel, TTransitiveModel> extractor)
    {
        _enrichmentFactory = enrichmentFactory;
        _extractor = extractor;
    }

    public async Task HandleAsync(IEnrichmentContext<TKey, TModel, TState> context, CancellationToken cancellationToken)
    {
        var transitiveModels = context.Models.Select(_extractor);

        await _enrichmentFactory
            .Create<TTransitiveKey, TTransitiveModel, TState>()
            .EnrichAsync(context.State, transitiveModels, cancellationToken)
            .AsTask(cancellationToken);
    }
}
