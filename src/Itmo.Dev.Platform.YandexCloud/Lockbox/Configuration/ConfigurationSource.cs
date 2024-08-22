namespace Itmo.Dev.Platform.YandexCloud.Lockbox.Configuration;

internal class ConfigurationSource : IConfigurationSource
{
    private readonly IConfigurationProvider _provider;

    public ConfigurationSource(IConfigurationProvider provider)
    {
        _provider = provider;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return _provider;
    }
}