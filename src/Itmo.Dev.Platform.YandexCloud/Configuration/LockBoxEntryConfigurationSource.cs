using Itmo.Dev.Platform.YandexCloud.Models;
using Microsoft.Extensions.Configuration;

namespace Itmo.Dev.Platform.YandexCloud.Configuration;

internal class LockBoxEntryConfigurationSource : IConfigurationSource
{
    private readonly LockBoxEntry[] _entries;

    public LockBoxEntryConfigurationSource(LockBoxEntry[] entries)
    {
        _entries = entries;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new LockBoxEntryConfigurationProvider(_entries);
    }
}