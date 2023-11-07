using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Itmo.Dev.Platform.Testing.ApplicationFactories;

public abstract class ConfigurableWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(Environment.CurrentDirectory)
            .ConfigureServices(ConfigureServices)
            .UseStartup<TStartup>();
    }

    protected virtual void ConfigureServices(IServiceCollection collection) { }
}