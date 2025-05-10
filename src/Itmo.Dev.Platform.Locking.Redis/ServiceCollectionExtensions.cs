using Itmo.Dev.Platform.Locking.Redis.Configuration;
using Itmo.Dev.Platform.Locking.Redis.FormattingStrategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Itmo.Dev.Platform.Locking.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformRedisLocking(
        this IServiceCollection collection,
        Func<IOptionsConfigurator, IRedisLockingConfigurator> config)
    {
        var configurator = new RedisLockingConfigurator(collection);
        config.Invoke(configurator);

        collection.AddSingleton<ILockingService, RedisLockService>();

        collection.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptionsMonitor<RedisLockingOptions>>().CurrentValue;

            return RedLockFactory.Create(
                existingMultiplexers: [ConnectionMultiplexer.Connect(options.Endpoint)],
                loggerFactory: provider.GetRequiredService<ILoggerFactory>());
        });

        return collection;
    }

    public static IRedisLockingConfigurator WithNewtonsoftKeyFormatter(
        this IKeyFormattingStrategyConfigurator configurator)
    {
        return configurator.WithKeyFormatter<NewtonsoftFormattingStrategy>();
    }

    public static IRedisLockingConfigurator WithHashCodeKeyFormatter(
        this IKeyFormattingStrategyConfigurator configurator)
    {
        return configurator.WithKeyFormatter(new HashCodeFormattingStrategy());
    }
}
