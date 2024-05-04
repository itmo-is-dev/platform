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

    public IEnrichmentConfigurator Enrich<TKey, TModel>(
        Action<IEnrichmentTypeConfigurator<TKey, TModel>>? configuration)
        where TKey : notnull
        where TModel : IEnrichedModel<TKey>
    {
        _collection.AddScoped<IEnrichmentProcessor<TKey, TModel>, EnrichmentProcessor<TKey, TModel>>();

        if (configuration is null)
            return this;

        var configurator = new EnrichmentTypeConfigurator<TKey, TModel>(_collection);
        configuration.Invoke(configurator);

        return this;
    }
}

file class EnrichmentTypeConfigurator<TKey, TModel> : IEnrichmentTypeConfigurator<TKey, TModel>
    where TKey : notnull
    where TModel : IEnrichedModel<TKey>
{
    private readonly IServiceCollection _collection;

    public EnrichmentTypeConfigurator(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IEnrichmentTypeConfigurator<TKey, TModel> WithHandler<THandler>()
        where THandler : class, IEnrichmentHandler<TKey, TModel>
    {
        _collection.TryAddEnumerable(
            ServiceDescriptor.Scoped<IEnrichmentHandler<TKey, TModel>, THandler>());

        return this;
    }
}