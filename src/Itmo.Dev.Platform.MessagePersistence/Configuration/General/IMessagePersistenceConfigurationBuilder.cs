using Itmo.Dev.Platform.MessagePersistence.Configuration.Buffering;
using Itmo.Dev.Platform.MessagePersistence.Configuration.MessageHandlers;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Itmo.Dev.Platform.MessagePersistence.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration.General;

public interface IMessagePersistenceDefaultPublisherConfigurationSelector
{
    IMessagePersistencePersistenceConfigurationSelector WithDefaultPublisherOptions(
        Action<OptionsBuilder<MessagePersistencePublisherOptions>> action);

    IMessagePersistencePersistenceConfigurationSelector WithDefaultPublisherOptions(string sectionPath)
    {
        return WithDefaultPublisherOptions(builder =>
        {
            builder.BindConfiguration(sectionPath, binder => binder.BindNonPublicProperties = false);
        });
    }
}

public interface IMessagePersistencePersistenceConfigurationSelector
{
    IMessagePersistenceBufferingGroupConfigurator UsePersistenceConfigurator(
        IMessagePersistencePersistenceConfigurator configurator);
}

public interface IMessagePersistenceBufferingGroupConfigurator : IMessagePersistenceConfigurationBuilder
{
    IMessagePersistenceBufferingGroupConfigurator AddBufferingGroup(
        Func<IMessagePersistenceBufferingNameSelector, IMessagePersistenceBufferingBuilder> action);
}

public interface IMessagePersistenceConfigurationBuilder
{
    IMessagePersistenceConfigurationBuilder AddMessage(
        Func<IMessagePersistenceHandlerNameConfigurator, IMessagePersistenceHandlerBuilder> configuration);
}
