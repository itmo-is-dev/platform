using Itmo.Dev.Platform.YandexCloud.Lockbox.Models;

namespace Itmo.Dev.Platform.YandexCloud.Lockbox.Configuration;

internal class LockBoxEntryConfigurationProvider : ConfigurationProvider
{
    public LockBoxEntryConfigurationProvider(IEnumerable<LockBoxEntry> entries)
    {
        Data = new Dictionary<string, string?>();
        UpdateValues(entries);
    }

    public void UpdateValues(IEnumerable<LockBoxEntry> entries)
    {
        var updated = false;

        var entryData = entries.ToDictionary(
            x => Normalize(x.Key),
            x => (string?)x.Value,
            StringComparer.OrdinalIgnoreCase);

        foreach ((string key, string? oldValue) in Data)
        {
            if (entryData.TryGetValue(key, out string? newValue))
            {
                if (StringComparer.Ordinal.Equals(oldValue, newValue) is false)
                    updated = true;
            }
            else
            {
                updated = true;
            }
        }

        updated = updated || entryData.Keys.Any(k => Data.ContainsKey(k) is false);

        Data = entryData;

        if (updated)
            OnReload();
    }

    private static string Normalize(string key)
    {
        return key.Replace("__", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);
    }
}