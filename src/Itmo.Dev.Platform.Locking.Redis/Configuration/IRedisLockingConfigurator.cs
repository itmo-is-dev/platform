using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Locking.Redis.Configuration;

public interface IOptionsConfigurator
{
    IDefaultKeyFormatterConfigurator WithOptions(Action<OptionsBuilder<RedisLockingOptions>> action);
}

public interface IDefaultKeyFormatterConfigurator
{
    IKeyFormatterConfigurator WithDefaultKeyFormatter<TFormatter>()
        where TFormatter : class, ILockingKeyFormatter;

    IKeyFormatterConfigurator WithDefaultKeyFormatter<TFormatter>(TFormatter formatter)
        where TFormatter : class, ILockingKeyFormatter;
}

public interface IKeyFormatterConfigurator : IRedisLockingConfigurator
{
    IKeyFormatterConfigurator WithKeyFormatter<TKey, TFormatter>()
        where TFormatter : class, ILockingKeyFormatter;

    IKeyFormatterConfigurator WithKeyFormatter<TKey, TFormatter>(TFormatter formatter)
        where TFormatter : class, ILockingKeyFormatter;
}

public interface IRedisLockingConfigurator;

internal class RedisLockingConfigurator :
    IOptionsConfigurator,
    IDefaultKeyFormatterConfigurator,
    IKeyFormatterConfigurator
{
    private readonly IServiceCollection _collection;

    public RedisLockingConfigurator(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IDefaultKeyFormatterConfigurator WithOptions(Action<OptionsBuilder<RedisLockingOptions>> action)
    {
        var builder = _collection
            .AddOptions<RedisLockingOptions>()
            .ValidateOnStart()
            .ValidateDataAnnotations();

        action.Invoke(builder);

        return this;
    }

    public IKeyFormatterConfigurator WithDefaultKeyFormatter<TFormatter>()
        where TFormatter : class, ILockingKeyFormatter
    {
        _collection.AddSingleton<ILockingKeyFormatter, TFormatter>();
        return this;
    }

    public IKeyFormatterConfigurator WithDefaultKeyFormatter<TFormatter>(TFormatter formatter)
        where TFormatter : class, ILockingKeyFormatter
    {
        _collection.AddSingleton<ILockingKeyFormatter>(formatter);
        return this;
    }

    public IKeyFormatterConfigurator WithKeyFormatter<TKey, TFormatter>()
        where TFormatter : class, ILockingKeyFormatter
    {
        _collection.AddKeyedSingleton<ILockingKeyFormatter, TFormatter>(typeof(TKey));
        return this;
    }

    public IKeyFormatterConfigurator WithKeyFormatter<TKey, TFormatter>(TFormatter formatter)
        where TFormatter : class, ILockingKeyFormatter
    {
        _collection.AddKeyedSingleton<ILockingKeyFormatter>(
            serviceKey: typeof(TKey),
            implementationInstance: formatter);

        return this;
    }
}
