using Itmo.Dev.Platform.MessagePersistence.Configuration.Builders;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration.General;

public interface IMessagePersistencePersistenceConfigurationSelector
{
    IMessagePersistenceConfigurationBuilder UsePersistenceConfigurator(
        IMessagePersistencePersistenceConfigurator configurator);
}

public interface IMessagePersistenceConfigurationBuilder
{
    IMessagePersistenceConfigurationBuilder AddMessage(
        Func<IMessagePersistenceHandlerNameConfigurator, IMessagePersistenceHandlerBuilder> configuration);
}