using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Kafka.QualifiedServices;

public class NamedOptionsQualifiedService<TKey, TValue, TOptions>
    : IKeyValueQualifiedService<TKey, TValue, TOptions>
{
    private readonly string _name;

    public NamedOptionsQualifiedService(string name)
    {
        _name = name;
    }

    public TOptions Resolve(IServiceProvider provider)
    {
        var monitor = provider.GetRequiredService<IOptionsMonitor<TOptions>>();
        return monitor.Get(_name);
    }
}