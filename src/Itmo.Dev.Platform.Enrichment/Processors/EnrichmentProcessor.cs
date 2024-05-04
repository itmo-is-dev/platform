using Itmo.Dev.Platform.Enrichment.Contexts;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.Enrichment.Processors;

internal class EnrichmentProcessor<TKey, TModel> : IEnrichmentProcessor<TKey, TModel>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    private readonly IEnumerable<IEnrichmentHandler<TKey, TModel>> _handlers;
    private readonly ILogger<EnrichmentProcessor<TKey, TModel>> _logger;

    public EnrichmentProcessor(
        IEnumerable<IEnrichmentHandler<TKey, TModel>> handlers,
        ILogger<EnrichmentProcessor<TKey, TModel>> logger)
    {
        _handlers = handlers;
        _logger = logger;
    }

    public async IAsyncEnumerable<TModel> EnrichAsync(
        IEnumerable<TModel> models,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        models = models.ToArray();

        var context = new EnrichmentContext<TKey, TModel>(models);

        var tasks = _handlers.Select(
            async handler =>
            {
                try
                {
                    await handler.HandleAsync(context, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(
                        e,
                        "Failed to execute enrichment handler with type = {Type}",
                        handler.GetType());
                }
            });

        await Task.WhenAll(tasks);

        foreach (TModel model in models)
        {
            yield return model;
        }
    }
}