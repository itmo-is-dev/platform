using Itmo.Dev.Platform.MessagePersistence.Internal.Tools;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Configuration;

internal class MessagePersistenceBuilder :
    Config.General.IDefaultPublisherStep,
    Config.General.IPersistenceStep,
    Config.General.IBufferingGroupStep
{
    private readonly IServiceCollection _collection;

    public MessagePersistenceBuilder(IServiceCollection collection)
    {
        _collection = collection;
    }

    public Config.General.IPersistenceStep WithDefaultPublisherOptions(
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

    public Config.General.IBufferingGroupStep UsePersistenceConfigurator(
        IMessagePersistencePersistenceConfigurator configurator)
    {
        configurator.Apply(_collection);
        return this;
    }

    public Config.General.IBufferingGroupStep AddBufferingGroup(
        Func<Config.Buffering.INameStep, Config.Buffering.IFinalStep> action)
    {
        var builder = new MessagePersistenceBufferingBuilder(_collection);
        action.Invoke(builder);

        return this;
    }

    public IMessagePersistenceBuilder AddMessage(
        Func<Config.Message.INameStep, Config.Message.IFinalStep> configuration)
    {
        var configurator = new Config.Message.NameStep(_collection);
        configuration.Invoke(configurator).Build();

        return this;
    }
}
