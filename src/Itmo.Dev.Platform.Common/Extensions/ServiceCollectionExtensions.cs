using Itmo.Dev.Platform.Common.Configurations;
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
    public static IServiceCollection AddPlatform(
        this IServiceCollection collection,
        Func<PlatformCommonConfiguration.ISerializerStep, PlatformCommonConfiguration.IFinalStep> configuration)
    {
        collection.AddPlatformLifetimes();
        collection.AddUtcDateTimeProvider();

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

        var configurator = new PlatformCommonBuilder(collection);
        configuration(configurator);

        return collection;
    }

    public static IServiceCollection AddUtcDateTimeProvider(this IServiceCollection collection)
    {
        collection.TryAddSingleton<IDateTimeProvider, UtcDateTimeProvider>();
        return collection;
    }

    internal static IServiceCollection AddHostedServiceUnsafe<THostedService>(
        this IServiceCollection collection,
        Func<IServiceProvider, THostedService> factory)
        where THostedService : class, IHostedService
    {
        var descriptor = ServiceDescriptor.Singleton<IHostedService>(factory);
        collection.Add(descriptor);

        return collection;
    }
}
