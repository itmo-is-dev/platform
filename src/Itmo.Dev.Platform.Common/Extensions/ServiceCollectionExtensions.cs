using Itmo.Dev.Platform.Common.DateTime;
using Itmo.Dev.Platform.Common.Lifetime.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatform(this IServiceCollection collection)
    {
        collection.AddPlatformLifetimes();
        return collection;
    }

    public static IServiceCollection AddUtcDateTimeProvider(this IServiceCollection collection)
        => collection.AddSingleton<IDateTimeProvider, UtcDateTimeProvider>();
}