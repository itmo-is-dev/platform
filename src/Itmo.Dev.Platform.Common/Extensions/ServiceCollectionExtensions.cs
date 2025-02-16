using Itmo.Dev.Platform.Common.DateTime;
using Itmo.Dev.Platform.Common.Lifetime.Extensions;
using Itmo.Dev.Platform.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        return collection;
    }

    public static IServiceCollection AddUtcDateTimeProvider(this IServiceCollection collection)
        => collection.AddSingleton<IDateTimeProvider, UtcDateTimeProvider>();
}