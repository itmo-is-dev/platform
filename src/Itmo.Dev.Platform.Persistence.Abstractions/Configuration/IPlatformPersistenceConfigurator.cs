using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Itmo.Dev.Platform.Persistence.Abstractions.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Persistence.Abstractions.Configuration;

public interface IPlatformPersistenceConnectionProviderConfigurator
{
    IPlatformPersistenceTransactionProviderConfigurator UseConnectionProvider<T>()
        where T : class, IPersistenceConnectionProvider;
}

public interface IPlatformPersistenceTransactionProviderConfigurator
{
    IPlatformPersistenceConfigurator UseTransactionProvider<T>()
        where T : class, IPersistenceTransactionProvider;
}

public interface IPlatformPersistenceConfigurator
{
    IServiceCollection Services { get; }
}