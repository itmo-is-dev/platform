using Itmo.Dev.Platform.MessagePersistence.Configuration.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration.General;

internal class MessagePersistenceConfigurationBuilder :
    IMessagePersistencePersistenceConfigurationSelector,
    IMessagePersistenceConfigurationBuilder
{
    private readonly IServiceCollection _collection;

    public MessagePersistenceConfigurationBuilder(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IMessagePersistenceConfigurationBuilder UsePersistenceConfigurator(
        IMessagePersistencePersistenceConfigurator configurator)
    {
        configurator.Apply(_collection);
        return this;
    }

    public IMessagePersistenceConfigurationBuilder AddMessage(
        Func<IMessagePersistenceHandlerNameConfigurator, IMessagePersistenceHandlerBuilder> configuration)
    {
        var configurator = new MessagePersistenceHandlerNameConfigurator(_collection);
        configuration.Invoke(configurator).Build();

        return this;
    }
}