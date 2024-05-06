using Itmo.Dev.Platform.Enrichment.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Itmo.Dev.Platform.Enrichment.Configuration;

internal class EnrichmentConfigurator : IEnrichmentConfigurator
{
    private readonly IServiceCollection _collection;

    public EnrichmentConfigurator(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IEnrichmentConfigurator Enrich<TKey, TModel, TState>(
        Action<IEnrichmentTypeConfigurator<TKey, TModel, TState>>? configuration)
        where TKey : notnull
        where TModel : IEnrichedModel<TKey>
    {
        _collection.AddScoped<IEnrichmentProcessor<TKey, TModel, TState>, EnrichmentProcessor<TKey, TModel, TState>>();

        if (configuration is null)
            return this;

        var configurator = new EnrichmentTypeConfigurator<TKey, TModel, TState>(_collection);
        configuration.Invoke(configurator);

        return this;
    }
}

file class EnrichmentTypeConfigurator<TKey, TModel, TState> : IEnrichmentTypeConfigurator<TKey, TModel, TState>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    private readonly IServiceCollection _collection;

    public EnrichmentTypeConfigurator(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IEnrichmentTypeConfigurator<TKey, TModel, TState> WithHandler<THandler>()
        where THandler : class, IEnrichmentHandler<TKey, TModel, TState>
    {
        _collection.TryAddEnumerable(
            ServiceDescriptor.Scoped<IEnrichmentHandler<TKey, TModel, TState>, THandler>());

        return this;
    }
}