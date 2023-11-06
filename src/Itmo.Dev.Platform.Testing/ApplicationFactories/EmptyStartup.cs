using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Testing.ApplicationFactories;

public class EmptyStartup
{
    public virtual void ConfigureServices(IServiceCollection services) { }

    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env) { }
}