using Itmo.Dev.Platform.MessagePersistence.Internal.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Internal.Options;
using Itmo.Dev.Platform.MessagePersistence.Internal.Tools;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Itmo.Dev.Platform.MessagePersistence;

public static partial class MessagePersistenceConfiguration
{
    public static partial class Message
    {
        internal class NameStep(IServiceCollection collection) : INameStep
        {
            public IConfigurationStep Called(string messageName) => new ConfigurationStep(messageName, collection);
        }
    }
}

file class ConfigurationStep(string messageName, IServiceCollection collection) : Config.Message.IConfigurationStep
{
    public Config.Message.IMessageStep WithConfiguration(
        Action<OptionsBuilder<MessagePersistenceHandlerOptions>> action)
    {
        var builder = collection
            .AddOptions<MessagePersistenceHandlerOptions>(messageName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        action.Invoke(builder);

        return new MessageStep(messageName, collection);
    }
}

file class MessageStep(string messageName, IServiceCollection collection) : Config.Message.IMessageStep
{
    public Config.Message.IHandlerStep<TMessage> WithMessage<TMessage>()
        where TMessage : IPersistedMessage<TMessage>
    {
        var builder = new PersistedMessageBuilder<TMessage>(messageName, collection);
        IPersistedMessageFinalBuilder _ = TMessage.Configure(builder);

        return new HandlerStep<TMessage>(messageName, collection);
    }
}

file class HandlerStep<TMessage>(string messageName, IServiceCollection collection)
    : Config.Message.IHandlerStep<TMessage>
    where TMessage : IPersistedMessage
{
    public Config.Message.IBufferingGroupStep HandleBy<THandler>()
        where THandler : class, IPersistedMessageHandler<TMessage>
    {
        collection.AddKeyedScoped<IPersistedMessageHandler<TMessage>, THandler>(messageName);

        return new MessagePersistenceMessageBuilder<TMessage>(messageName, collection);
    }

    public Config.Message.IBufferingGroupStep HandleBy<THandler>(
        Func<IServiceProvider, string, THandler> implementationFactory)
        where THandler : class, IPersistedMessageHandler<TMessage>
    {
        collection.AddKeyedScoped<IPersistedMessageHandler<TMessage>, THandler>(
            messageName,
            implementationFactory: (provider, _) => implementationFactory.Invoke(provider, messageName));

        return new MessagePersistenceMessageBuilder<TMessage>(messageName, collection);
    }
}

file class MessagePersistenceMessageBuilder<TMessage>(
    string messageName,
    IServiceCollection collection)
    : Config.Message.IBufferingGroupStep
    where TMessage : IPersistedMessage
{
    private string? _bufferingGroup;

    public Config.Message.IFinalStep WithBufferingGroup(string bufferingGroupName)
    {
        _bufferingGroup = bufferingGroupName;
        return this;
    }

    public void Build()
    {
        collection.Configure<MessagePersistenceOptions>(options =>
            options.AddPersistedMessage(typeof(TMessage), messageName));

        collection.Configure<MessagePersistencePublisherOptions>(
            _bufferingGroup ?? MessagePersistenceConstants.DefaultPublisherName,
            configureOptions: options => options.MessageNames.Add(messageName));

        collection
            .AddOptions<PersistedMessageOptions>(messageName)
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Configure(options =>
            {
                options.IsInitialized = true;
                options.MessageType = typeof(TMessage);
                options.BufferGroup = _bufferingGroup;
            });
    }
}
