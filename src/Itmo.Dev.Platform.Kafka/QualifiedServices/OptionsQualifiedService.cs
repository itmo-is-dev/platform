using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Kafka.QualifiedServices;

public class OptionsQualifiedService<TKey, TValue, TOptions> : IKeyValueQualifiedService<TKey, TValue, TOptions>
{
    public TOptions Resolve(IServiceProvider provider)
    {
        var monitor = provider.GetRequiredService<IOptionsMonitor<TOptions>>();
        return monitor.CurrentValue;
    }
}