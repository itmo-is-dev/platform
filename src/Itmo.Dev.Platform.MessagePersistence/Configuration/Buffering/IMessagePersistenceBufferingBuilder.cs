using Itmo.Dev.Platform.MessagePersistence.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Itmo.Dev.Platform.MessagePersistence;

public interface IMessagePersistenceBufferingNameSelector
{
    IMessagePersistenceBufferingPublisherConfigurationSelector Called(string name);
}

public interface IMessagePersistenceBufferingPublisherConfigurationSelector
{
    IMessagePersistenceBufferingStepSelector WithPublisherConfiguration(
        Action<OptionsBuilder<MessagePersistencePublisherOptions>> action);

    IMessagePersistenceBufferingStepSelector WithPublisherConfiguration(string sectionName)
    {
        return WithPublisherConfiguration(builder => builder.BindConfiguration(
            sectionName,
            binder => binder.BindNonPublicProperties = false));
    }
}

public interface IMessagePersistenceBufferingStepSelector : IMessagePersistenceBufferingBuilder
{
    internal IServiceCollection Services { get; }
    
    internal string BufferGroupName { get; }
    
    internal IMessagePersistenceBufferingStepSelector WithStep(BufferStepOptions step);
}

public interface IMessagePersistenceBufferingBuilder;
