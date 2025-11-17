using Itmo.Dev.Platform.MessagePersistence.Configuration.Buffering;
using Itmo.Dev.Platform.MessagePersistence.Configuration.MessageHandlers;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Itmo.Dev.Platform.MessagePersistence.Services;
using Itmo.Dev.Platform.MessagePersistence.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration.General;

internal class MessagePersistenceConfigurationBuilder :
    IMessagePersistenceDefaultPublisherConfigurationSelector,
    IMessagePersistencePersistenceConfigurationSelector,
    IMessagePersistenceBufferingGroupConfigurator,
    IMessagePersistenceConfigurationBuilder
{
    private readonly IServiceCollection _collection;

    public MessagePersistenceConfigurationBuilder(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IMessagePersistencePersistenceConfigurationSelector WithDefaultPublisherOptions(
        Action<OptionsBuilder<MessagePersistencePublisherOptions>> action)
    {
        var builder = _collection
            .AddOptions<MessagePersistencePublisherOptions>(MessagePersistenceConstants.DefaultPublisherName)
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Configure(options => options.IsInitialized = true);

        action.Invoke(builder);

        return this;
    }

    public IMessagePersistenceBufferingGroupConfigurator UsePersistenceConfigurator(
        IMessagePersistencePersistenceConfigurator configurator)
    {
        configurator.Apply(_collection);
        return this;
    }

    public IMessagePersistenceBufferingGroupConfigurator AddBufferingGroup(
        Func<IMessagePersistenceBufferingNameSelector, IMessagePersistenceBufferingBuilder> action)
    {
        var builder = new MessagePersistenceBufferingBuilder(_collection);
        action.Invoke(builder);

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
