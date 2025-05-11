using Itmo.Dev.Platform.Locking.FormattingStrategies;
using Itmo.Dev.Platform.Locking.Redis.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedLockNet.SERedis;
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

    public static IKeyFormatterConfigurator WithNewtonsoftDefaultKeyFormatter(
        this IDefaultKeyFormatterConfigurator configurator)
    {
        return configurator.WithDefaultKeyFormatter<NewtonsoftLockingKeyFormatter>();
    }

    public static IKeyFormatterConfigurator WithHashCodeDefaultKeyFormatter(
        this IDefaultKeyFormatterConfigurator configurator)
    {
        return configurator.WithDefaultKeyFormatter(new HashCodeLockingKeyFormatter());
    }

    public static IRedisLockingConfigurator WithNewtonsoftKeyFormatter<TKey>(
        this IKeyFormatterConfigurator configurator)
    {
        return configurator.WithKeyFormatter<TKey, NewtonsoftLockingKeyFormatter>();
    }

    public static IRedisLockingConfigurator WithHashCodeKeyFormatter<TKey>(
        this IKeyFormatterConfigurator configurator)
    {
        return configurator.WithKeyFormatter<TKey, HashCodeLockingKeyFormatter>(new HashCodeLockingKeyFormatter());
    }
}
