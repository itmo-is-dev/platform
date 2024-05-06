using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Itmo.Dev.Platform.Persistence.Abstractions.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Persistence.Abstractions.Configuration;

internal class PlatformPersistenceConfigurator :
    IPlatformPersistenceConnectionProviderConfigurator,
    IPlatformPersistenceTransactionProviderConfigurator,
    IPlatformPersistenceConfigurator
{
    public PlatformPersistenceConfigurator(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }

    public IPlatformPersistenceTransactionProviderConfigurator UseConnectionProvider<T>()
        where T : class, IPersistenceConnectionProvider
    {
        Services.AddScoped<IPersistenceConnectionProvider, T>();
        return this;
    }

    public IPlatformPersistenceConfigurator UseTransactionProvider<T>()
        where T : class, IPersistenceTransactionProvider
    {
        Services.AddScoped<IPersistenceTransactionProvider, T>();
        return this;
    }
}