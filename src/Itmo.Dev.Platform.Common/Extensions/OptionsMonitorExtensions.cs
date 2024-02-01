using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Common.Extensions;

public static class OptionsMonitorExtensions
{
    public static IDisposable? OnNamedChange<T>(this IOptionsMonitor<T> monitor, string name, Action<T> action)
    {
        return monitor.OnChange((value, changedName) =>
        {
            if (string.Equals(changedName, name, StringComparison.InvariantCultureIgnoreCase))
                action.Invoke(value);
        });
    }
}