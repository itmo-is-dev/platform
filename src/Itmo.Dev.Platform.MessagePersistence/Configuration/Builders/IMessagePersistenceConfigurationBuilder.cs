using Microsoft.Extensions.Configuration;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration.Builders;

public interface IMessagePersistencePersistenceConfigurator
{
    IMessagePersistenceConfigurationBuilder ConfigurePersistence(
        IConfiguration configuration,
        Action<MessagePersistencePersistenceOptions>? action = null);
}

public interface IMessagePersistenceConfigurationBuilder
{
    IMessagePersistenceConfigurationBuilder AddMessage(
        Func<IMessagePersistenceHandlerNameConfigurator, IMessagePersistenceHandlerBuilder> configuration);
}