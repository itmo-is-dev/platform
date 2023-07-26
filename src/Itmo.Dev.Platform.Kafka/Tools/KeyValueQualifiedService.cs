using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Tools;

internal class KeyValueQualifiedService<TKey, TValue, TService> : IServiceResolver<TService>
{
    private readonly Type _implementationType;

    public KeyValueQualifiedService(Type implementationType)
    {
        _implementationType = implementationType;
    }

    public TService Resolve(IServiceProvider provider)
    {
        return (TService)provider.GetRequiredService(_implementationType);
    }
}