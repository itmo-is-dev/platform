using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Itmo.Dev.Platform.Testing.ApplicationFactories;

public class PlatformWebApplicationBuilder<TStartup>
    where TStartup : class
{
    private readonly List<Action<IServiceCollection, IConfiguration>> _serviceConfigurations = [];
    private readonly List<Action<IConfigurationBuilder>> _configurationConfigurations = [];

    public PlatformWebApplicationBuilder<TStartup> ConfigureServices(Action<IServiceCollection, IConfiguration> action)
    {
        _serviceConfigurations.Add(action);
        return this;
    }

    public PlatformWebApplicationBuilder<TStartup> ConfigureServices(Action<IServiceCollection> action)
        => ConfigureServices((services, _) => action(services));

    public PlatformWebApplicationBuilder<TStartup> ConfigureConfiguration(Action<IConfigurationBuilder> action)
    {
        _configurationConfigurations.Add(action);
        return this;
    }

    public PlatformWebApplicationBuilder<TStartup> AddConfigurationEntry(string key, object value)
    {
        _configurationConfigurations.Add(builder => builder.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                [key] = value.ToString(),
            }));

        return this;
    }

    public PlatformWebApplicationBuilder<TStartup> AddConfigurationJson(string configuration)
    {
        _configurationConfigurations.Add(builder =>
        {
            var ms = new MemoryStream();
            ms.Write(Encoding.Default.GetBytes(configuration));
            ms.Position = 0;

            builder.AddJsonStream(ms);
        });

        return this;
    }

    public WebApplicationFactory<TStartup> Build()
    {
        return new WebApplicationFactory<TStartup>().WithWebHostBuilder(builder => builder
            .ConfigureServices((context, services)
                => _serviceConfigurations.ForEach(action => action(services, context.Configuration)))
            .ConfigureAppConfiguration((_, configurationBuilder)
                => _configurationConfigurations.ForEach(action => action(configurationBuilder))));
    }
}
