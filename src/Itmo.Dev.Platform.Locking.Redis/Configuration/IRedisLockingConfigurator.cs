using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Locking.Redis.Configuration;

public interface IOptionsConfigurator
{
    IKeyFormattingStrategyConfigurator WithOptions(Action<OptionsBuilder<RedisLockingOptions>> action);
}

public interface IKeyFormattingStrategyConfigurator
{
    IRedisLockingConfigurator WithKeyFormatter<T>()
        where T : class, IKeyFormattingStrategy;

    IRedisLockingConfigurator WithKeyFormatter<T>(T value)
        where T : class, IKeyFormattingStrategy;
}

public interface IRedisLockingConfigurator { }

internal class RedisLockingConfigurator :
    IOptionsConfigurator,
    IKeyFormattingStrategyConfigurator,
    IRedisLockingConfigurator
{
    private readonly IServiceCollection _collection;

    public RedisLockingConfigurator(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IKeyFormattingStrategyConfigurator WithOptions(Action<OptionsBuilder<RedisLockingOptions>> action)
    {
        var builder = _collection
            .AddOptions<RedisLockingOptions>()
            .ValidateOnStart()
            .ValidateDataAnnotations();

        action.Invoke(builder);

        return this;
    }

    public IRedisLockingConfigurator WithKeyFormatter<T>()
        where T : class, IKeyFormattingStrategy
    {
        _collection.AddSingleton<IKeyFormattingStrategy, T>();
        return this;
    }

    public IRedisLockingConfigurator WithKeyFormatter<T>(T value)
        where T : class, IKeyFormattingStrategy
    {
        _collection.AddSingleton<IKeyFormattingStrategy>(value);
        return this;
    }
}
