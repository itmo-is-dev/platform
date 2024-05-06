using Itmo.Dev.Platform.Common.Features;
using Itmo.Dev.Platform.Persistence.Abstractions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Persistence.Abstractions.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformPersistence(
        this IServiceCollection collection,
        Func<IPlatformPersistenceConnectionProviderConfigurator, IPlatformPersistenceConfigurator> configuration)
    {
        collection.AddPlatformFeature<PlatformPersistenceFeature>();
        
        var configurator = new PlatformPersistenceConfigurator(collection);
        configuration.Invoke(configurator);

        return collection;
    }
}