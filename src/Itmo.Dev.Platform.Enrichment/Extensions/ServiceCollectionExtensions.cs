using Itmo.Dev.Platform.Common.Features;
using Itmo.Dev.Platform.Enrichment.Configuration;
using Itmo.Dev.Platform.Enrichment.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Enrichment.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformEnrichment(
        this IServiceCollection collection,
        Action<IEnrichmentConfigurator> configuration)
    {
        collection.AddPlatformFeature<PlatformEnrichmentFeature>();

        var configurator = new EnrichmentConfigurator(collection);
        configuration.Invoke(configurator);

        return collection;
    }
}