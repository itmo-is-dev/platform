using Itmo.Dev.Platform.MessagePersistence.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration.MessageHandlers;

internal class MessagePersistenceHandlerNameConfigurator : IMessagePersistenceHandlerNameConfigurator
{
    private readonly IServiceCollection _collection;

    public MessagePersistenceHandlerNameConfigurator(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IMessagePersistenceHandlerConfigurator Called(string messageName)
    {
        return new MessagePersistenceHandlerConfigurator(messageName, _collection);
    }
}

internal class MessagePersistenceHandlerConfigurator : IMessagePersistenceHandlerConfigurator
{
    private readonly string _messageName;
    private readonly IServiceCollection _collection;

    public MessagePersistenceHandlerConfigurator(
        string messageName,
        IServiceCollection collection)
    {
        _messageName = messageName;
        _collection = collection;
    }

    public IMessagePersistenceHandlerKeyConfigurator WithConfiguration(
        Action<OptionsBuilder<MessagePersistenceHandlerOptions>> action)
    {
        var builder = _collection
            .AddOptions<MessagePersistenceHandlerOptions>(_messageName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        action.Invoke(builder);

        return new MessagePersistenceHandlerKeyConfigurator(_messageName, _collection);
    }
}

internal class MessagePersistenceHandlerKeyConfigurator : IMessagePersistenceHandlerKeyConfigurator
{
    private readonly string _messageName;
    private readonly IServiceCollection _collection;

    public MessagePersistenceHandlerKeyConfigurator(
        string messageName,
        IServiceCollection collection)
    {
        _messageName = messageName;
        _collection = collection;
    }

    public IMessagePersistenceHandlerValueConfigurator<TKey> WithKey<TKey>()
    {
        return new MessagePersistenceHandlerHandlerValueConfigurator<TKey>(_messageName, _collection);
    }
}

internal class MessagePersistenceHandlerHandlerValueConfigurator<TKey> : IMessagePersistenceHandlerValueConfigurator<TKey>
{
    private readonly string _messageName;
    private readonly IServiceCollection _collection;

    public MessagePersistenceHandlerHandlerValueConfigurator(
        string messageName,
        IServiceCollection collection)
    {
        _messageName = messageName;
        _collection = collection;
    }

    public IMessagePersistenceHandlerConfigurator<TKey, TValue> WithValue<TValue>()
    {
        return new MessagePersistenceHandlerConfigurator<TKey, TValue>(_messageName, _collection);
    }
}

internal class MessagePersistenceHandlerConfigurator<TKey, TValue> :
    IMessagePersistenceHandlerConfigurator<TKey, TValue>
{
    private readonly string _messageName;
    private readonly IServiceCollection _collection;

    public MessagePersistenceHandlerConfigurator(
        string messageName,
        IServiceCollection collection)
    {
        _messageName = messageName;
        _collection = collection;
    }

    public IMessagePersistenceHandlerBufferingGroupBuilder HandleBy<THandler>()
        where THandler : class, IMessagePersistenceHandler<TKey, TValue>
    {
        _collection.AddKeyedScoped<IMessagePersistenceHandler<TKey, TValue>, THandler>(_messageName);

        return new MessagePersistenceHandlerBuilder<TKey, TValue, THandler>(_messageName, _collection);
    }

    public IMessagePersistenceHandlerBufferingGroupBuilder HandleBy<THandler>(
        Func<IServiceProvider, string, THandler> implementationFactory)
        where THandler : class, IMessagePersistenceHandler<TKey, TValue>
    {
        _collection.AddKeyedScoped<IMessagePersistenceHandler<TKey, TValue>, THandler>(
            _messageName,
            (p, _) => implementationFactory.Invoke(p, _messageName));

        return new MessagePersistenceHandlerBuilder<TKey, TValue, THandler>(_messageName, _collection);
    }
}

internal class MessagePersistenceHandlerBuilder<TKey, TValue, THandler> :
    IMessagePersistenceHandlerBufferingGroupBuilder,
    IMessagePersistenceHandlerBuilder
    where THandler : IMessagePersistenceHandler<TKey, TValue>
{
    private readonly string _messageName;
    private readonly IServiceCollection _collection;

    private string? _bufferingGroup;

    public MessagePersistenceHandlerBuilder(
        string messageName,
        IServiceCollection collection)
    {
        _messageName = messageName;
        _collection = collection;
    }

    public IMessagePersistenceHandlerBuilder WithBufferingGroup(string bufferingGroupName)
    {
        _bufferingGroup = bufferingGroupName;
        return this;
    }

    public void Build()
    {
        _collection.Configure<MessagePersistencePublisherOptions>(
            _bufferingGroup ?? MessagePersistenceConstants.DefaultPublisherName,
            options => options.MessageNames.Add(_messageName));

        _collection
            .AddOptions<PersistedMessageOptions>(_messageName)
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Configure(options =>
            {
                options.IsInitialized = true;
                options.KeyType = typeof(TKey);
                options.ValueType = typeof(TValue);
                options.BufferGroup = _bufferingGroup;
            });
    }
}
