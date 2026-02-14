using Itmo.Dev.Platform.MessagePersistence.Internal.Configuration.Migrations;
using Itmo.Dev.Platform.MessagePersistence.Internal.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Configuration;

internal sealed class PersistedMessageBuilder<TMessage>(
    string messageName,
    IServiceCollection serviceCollection
)
    : IPersistedMessageBuilder<TMessage>
    where TMessage : IPersistedMessage
{
    public IPersistedMessagePayloadBuilder<TMessage, TPayload> WithPayload<TPayload>()
        where TPayload : IPersistedMessagePayload<TPayload>
    {
        return new PersistedMessagePayloadBuilder<TMessage, TPayload>(
            messageName,
            PayloadVersion.Default,
            serviceCollection,
            new InitialPayloadMigrationConfigurationLink<TPayload>());
    }
}

file class PersistedMessagePayloadBuilder<TMessage, TPayload>(
    string messageName,
    PayloadVersion currentVersion,
    IServiceCollection serviceCollection,
    IPayloadMigrationConfigurationLink<TPayload> payloadConfigurationLink
)
    : IPersistedMessagePayloadBuilder<TMessage, TPayload>, IPersistedMessageFinalBuilder
    where TMessage : IPersistedMessage
    where TPayload : IPersistedMessagePayload<TPayload>
{
    public IPersistedMessagePayloadBuilder<TMessage, TNext> WithPayload<TNext>(PayloadVersion version)
        where TNext : IPersistedMessagePayload<TNext, TPayload>
    {
        return new PersistedMessagePayloadBuilder<TMessage, TNext>(
            messageName,
            version,
            serviceCollection,
            new VersionedPayloadMigrationConfigurationLink<TNext, TPayload>(payloadConfigurationLink, version));
    }

    public IPersistedMessageFinalBuilder CreatedWithInternal(Func<TPayload, TMessage> factory)
    {
        serviceCollection
            .AddOptions<PersistedMessageOptions>(messageName)
            .Configure(options =>
            {
                options.Version = currentVersion;
                options.PayloadType = typeof(TPayload);
                options.Factory = messagePayload => factory((TPayload)messagePayload);
            });

        serviceCollection.AddKeyedSingleton(
            messageName,
            implementationFactory: (provider, _) => payloadConfigurationLink.CreateLink(provider));

        return this;
    }
}
