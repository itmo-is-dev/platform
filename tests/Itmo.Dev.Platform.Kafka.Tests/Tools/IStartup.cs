using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Tests.Tools;

public interface IStartup
{
    void ConfigureServices(IServiceCollection services);

    void Configure(IApplicationBuilder app, IWebHostEnvironment env);
}