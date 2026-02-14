using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Internal.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Internal.Execution;
using Itmo.Dev.Platform.MessagePersistence.Internal.Factory;
using Itmo.Dev.Platform.MessagePersistence.Internal.Metrics;
using Itmo.Dev.Platform.MessagePersistence.Internal.Options;
using Itmo.Dev.Platform.MessagePersistence.Internal.Options.Validators;
using Itmo.Dev.Platform.MessagePersistence.Internal.Services;
using Itmo.Dev.Platform.MessagePersistence.Internal.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformMessagePersistence(
        this IServiceCollection collection,
        Func<Config.General.IDefaultPublisherStep, IMessagePersistenceBuilder> config)
    {
        collection
            .AddOptions<MessagePersistenceOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        collection.AddSingleton<MessagePersistenceRegistry>();
        collection.AddSingleton<IMessagePersistenceMetrics, MessagePersistenceMetrics>();
        collection.AddScoped<IPersistedMessageFactory, PersistedMessageFactory>();
        collection.AddScoped<IMessagePersistenceService, MessagePersistenceService>();
        collection.AddScoped<IMessagePersistencePublisher, MessagePersistencePublisher>();

        collection.AddScoped<IMessagePersistenceExecutor, MessagePersistenceExecutor>();
        collection.AddScoped<IMessagePersistenceBufferingExecutor, MessagePersistenceBufferingExecutor>();

        collection.AddSingleton<IValidateOptions<PersistedMessageOptions>, PersistedMessageBufferGroupValidator>();
        collection.AddSingleton<IValidateOptions<BufferGroupOptions>, BufferingGroupPublisherValidator>();
        collection.AddSingleton<IValidateOptions<MessagePersistenceOptions>, MessagePersistenceOptionsValidator>();

        collection.AddHostedServiceUnsafe(provider
            => ActivatorUtilities.CreateInstance<MessagePersistenceInitialPublishBackgroundService>(
                provider,
                MessagePersistenceConstants.DefaultPublisherName));

        var builder = new MessagePersistenceBuilder(collection);
        config.Invoke(builder);

        return collection;
    }

    internal static IServiceCollection AddPlatformMessagePersistenceHandler(
        this IServiceCollection collection,
        Func<Config.Message.INameStep, Config.Message.IFinalStep> configuration)
    {
        var configurator = new Config.Message.NameStep(collection);
        configuration.Invoke(configurator).Build();

        return collection;
    }
}
