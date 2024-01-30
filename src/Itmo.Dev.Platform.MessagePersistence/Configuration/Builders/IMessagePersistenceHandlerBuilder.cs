using Microsoft.Extensions.Configuration;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration.Builders;

public interface IMessagePersistenceHandlerNameConfigurator
{
    IMessagePersistenceHandlerConfigurator Called(string name);
}

public interface IMessagePersistenceHandlerConfigurator
{
    IMessagePersistenceHandlerKeyConfigurator WithConfiguration(
        IConfiguration configuration,
        Action<MessagePersistenceHandlerOptions>? options = null);
}

public interface IMessagePersistenceHandlerKeyConfigurator
{
    IMessagePersistenceValueConfigurator<TKey> WithKey<TKey>();
}

public interface IMessagePersistenceValueConfigurator<out TKey>
{
    IMessagePersistenceHandlerConfigurator<TKey, TValue> WithValue<TValue>();
}

public interface IMessagePersistenceHandlerConfigurator<out TKey, out TValue>
{
    IMessagePersistenceHandlerBuilder HandleBy<THandler>() where THandler : class, IMessagePersistenceHandler<TKey, TValue>;
}

public interface IMessagePersistenceHandlerBuilder
{
    void Build();
}