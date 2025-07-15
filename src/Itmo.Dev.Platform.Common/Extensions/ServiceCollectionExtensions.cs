using Itmo.Dev.Platform.Common.DateTime;
using Itmo.Dev.Platform.Common.Lifetime.Extensions;
using Itmo.Dev.Platform.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatform(this IServiceCollection collection)
    {
        collection.AddPlatformLifetimes();

        collection
            .AddOptions<PlatformOptions>()
            .Configure<IConfiguration>((op, root) =>
            {
                var aspEnvironment = root.GetSection("ASPNETCORE_ENVIRONMENT").Value;

                if (aspEnvironment is not null)
                    op.Environment = aspEnvironment;
            })
            .BindConfiguration("Platform");

        collection.RemoveAll(typeof(IOptionsFactory<>));
        collection.AddTransient(typeof(IOptionsFactory<>), typeof(PlatformOptionsFactory<>));
        
        return collection;
    }

    public static IServiceCollection AddUtcDateTimeProvider(this IServiceCollection collection)
        => collection.AddSingleton<IDateTimeProvider, UtcDateTimeProvider>();

    public static IServiceCollection AddHostedServiceUnsafe<THostedService>(
        this IServiceCollection collection,
        Func<IServiceProvider, THostedService> factory)
        where THostedService : class, IHostedService
    {
        var descriptor = ServiceDescriptor.Singleton<IHostedService>(factory);
        collection.Add(descriptor);

        return collection;
    }
}
