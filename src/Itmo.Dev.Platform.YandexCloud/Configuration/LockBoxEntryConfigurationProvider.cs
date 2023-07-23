using Itmo.Dev.Platform.YandexCloud.Models;
using Microsoft.Extensions.Configuration;

namespace Itmo.Dev.Platform.YandexCloud.Configuration;

internal class LockBoxEntryConfigurationProvider : ConfigurationProvider
{
    public LockBoxEntryConfigurationProvider(IEnumerable<LockBoxEntry> entries)
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (LockBoxEntry lockBoxEntry in entries)
        {
            data[Normalize(lockBoxEntry.Key)] = lockBoxEntry.Value;
        }

        Data = data;
    }

    private static string Normalize(string key)
    {
        return key.Replace("__", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);
    }
}