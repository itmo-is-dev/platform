using Itmo.Dev.Platform.Locking.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Locking.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformLockingInMemory(this IServiceCollection collection)
    {
        return collection.AddSingleton<ILockingService, InMemoryLockingService>();
    }
}