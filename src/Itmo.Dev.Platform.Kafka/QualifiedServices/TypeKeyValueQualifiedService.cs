using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.QualifiedServices;

internal class TypeKeyValueQualifiedService<TKey, TValue, TService> : IKeyValueQualifiedService<TKey, TValue, TService>
{
    private readonly Type _implementationType;

    public TypeKeyValueQualifiedService(Type implementationType)
    {
        _implementationType = implementationType;
    }

    public TService Resolve(IServiceProvider provider)
    {
        return (TService)provider.GetRequiredService(_implementationType);
    }
}