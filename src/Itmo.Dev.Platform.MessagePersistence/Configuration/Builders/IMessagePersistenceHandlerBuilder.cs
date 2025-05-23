using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration.Builders;

public interface IMessagePersistenceHandlerNameConfigurator
{
    IMessagePersistenceHandlerConfigurator Called(string name);
}

public interface IMessagePersistenceHandlerConfigurator
{
    IMessagePersistenceHandlerKeyConfigurator WithConfiguration(
        Action<OptionsBuilder<MessagePersistenceHandlerOptions>> action);

    IMessagePersistenceHandlerKeyConfigurator WithConfiguration(
        IConfiguration configuration,
        Action<MessagePersistenceHandlerOptions>? options = null)
    {
        return WithConfiguration(builder =>
        {
            builder.Bind(configuration);

            if (options is not null)
                builder.Configure(options);
        });
    }

    IMessagePersistenceHandlerKeyConfigurator WithConfiguration(string sectionPath)
    {
        return WithConfiguration(builder => builder.BindConfiguration(sectionPath));
    }
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
    IMessagePersistenceHandlerBuilder HandleBy<THandler>()
        where THandler : class, IMessagePersistenceHandler<TKey, TValue>;

    IMessagePersistenceHandlerBuilder HandleBy<THandler>(Func<IServiceProvider, string, THandler> implementationFactory)
        where THandler : class, IMessagePersistenceHandler<TKey, TValue>;
}

public interface IMessagePersistenceHandlerBuilder
{
    void Build();
}
