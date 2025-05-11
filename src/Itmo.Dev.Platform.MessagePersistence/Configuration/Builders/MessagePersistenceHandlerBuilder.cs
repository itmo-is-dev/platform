using Itmo.Dev.Platform.MessagePersistence.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration.Builders;

internal class MessagePersistenceHandlerNameConfigurator : IMessagePersistenceHandlerNameConfigurator
{
    private readonly IServiceCollection _collection;

    public MessagePersistenceHandlerNameConfigurator(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IMessagePersistenceHandlerConfigurator Called(string name)
    {
        return new MessagePersistenceHandlerConfigurator(name, _collection);
    }
}

internal class MessagePersistenceHandlerConfigurator : IMessagePersistenceHandlerConfigurator
{
    private readonly string _name;
    private readonly IServiceCollection _collection;

    public MessagePersistenceHandlerConfigurator(string name, IServiceCollection collection)
    {
        _name = name;
        _collection = collection;
    }

    public IMessagePersistenceHandlerKeyConfigurator WithConfiguration(
        Action<OptionsBuilder<MessagePersistenceHandlerOptions>> action)
    {
        var builder = _collection
            .AddOptions<MessagePersistenceHandlerOptions>(_name)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        action.Invoke(builder);

        return new MessagePersistenceHandlerKeyConfigurator(_name, _collection);
    }
}

internal class MessagePersistenceHandlerKeyConfigurator : IMessagePersistenceHandlerKeyConfigurator
{
    private readonly string _name;
    private readonly IServiceCollection _collection;

    public MessagePersistenceHandlerKeyConfigurator(string name, IServiceCollection collection)
    {
        _name = name;
        _collection = collection;
    }

    public IMessagePersistenceValueConfigurator<TKey> WithKey<TKey>()
    {
        return new MessagePersistenceHandlerValueConfigurator<TKey>(_name, _collection);
    }
}

internal class MessagePersistenceHandlerValueConfigurator<TKey> : IMessagePersistenceValueConfigurator<TKey>
{
    private readonly string _name;
    private readonly IServiceCollection _collection;

    public MessagePersistenceHandlerValueConfigurator(string name, IServiceCollection collection)
    {
        _name = name;
        _collection = collection;
    }

    public IMessagePersistenceHandlerConfigurator<TKey, TValue> WithValue<TValue>()
    {
        return new MessagePersistenceHandlerConfigurator<TKey, TValue>(_name, _collection);
    }
}

internal class MessagePersistenceHandlerConfigurator<TKey, TValue> :
    IMessagePersistenceHandlerConfigurator<TKey, TValue>
{
    private readonly string _name;
    private readonly IServiceCollection _collection;

    public MessagePersistenceHandlerConfigurator(string name, IServiceCollection collection)
    {
        _name = name;
        _collection = collection;
    }

    public IMessagePersistenceHandlerBuilder HandleBy<THandler>()
        where THandler : class, IMessagePersistenceHandler<TKey, TValue>
    {
        _collection.AddKeyedScoped<IMessagePersistenceHandler<TKey, TValue>, THandler>(_name);

        return new MessagePersistenceHandlerBuilder<TKey, TValue, THandler>(_name, _collection);
    }

    public IMessagePersistenceHandlerBuilder HandleBy<THandler>(
        Func<IServiceProvider, string, THandler> implementationFactory)
        where THandler : class, IMessagePersistenceHandler<TKey, TValue>
    {
        _collection.AddKeyedScoped<IMessagePersistenceHandler<TKey, TValue>, THandler>(
            _name,
            (p, _) => implementationFactory.Invoke(p, _name));

        return new MessagePersistenceHandlerBuilder<TKey, TValue, THandler>(_name, _collection);
    }
}

internal class MessagePersistenceHandlerBuilder<TKey, TValue, THandler> : IMessagePersistenceHandlerBuilder
    where THandler : IMessagePersistenceHandler<TKey, TValue>
{
    private readonly string _name;
    private readonly IServiceCollection _collection;

    public MessagePersistenceHandlerBuilder(string name, IServiceCollection collection)
    {
        _name = name;
        _collection = collection;
    }

    public void Build()
    {
        _collection.AddHostedService(provider =>
            ActivatorUtilities.CreateInstance<MessagePersistenceBackgroundService<TKey, TValue>>(provider, _name));
    }
}
