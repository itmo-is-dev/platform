using Itmo.Dev.Platform.Common.DateTime;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUtcDateTimeProvider(this IServiceCollection collection)
        => collection.AddSingleton<IDateTimeProvider, UtcDateTimeProvider>();
}