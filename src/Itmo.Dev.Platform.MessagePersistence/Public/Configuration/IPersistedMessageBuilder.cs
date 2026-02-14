namespace Itmo.Dev.Platform.MessagePersistence;

public interface IPersistedMessageBuilder<TMessage>
    where TMessage : IPersistedMessage
{
    IPersistedMessagePayloadBuilder<TMessage, TPayload> WithPayload<TPayload>()
        where TPayload : IPersistedMessagePayload<TPayload>;
}

public interface IPersistedMessagePayloadBuilder<TMessage, TPayload>
    where TMessage : IPersistedMessage
    where TPayload : IPersistedMessagePayload<TPayload>
{
    IPersistedMessagePayloadBuilder<TMessage, TNext> WithPayload<TNext>(PayloadVersion version)
        where TNext : IPersistedMessagePayload<TNext, TPayload>;

    internal IPersistedMessageFinalBuilder CreatedWithInternal(Func<TPayload, TMessage> factory);
}

public static class PersistedMessagePayloadBuilderExtensions
{
    public static IPersistedMessageFinalBuilder CreatedWith<TMessage, TPayload>(
        this IPersistedMessagePayloadBuilder<TMessage, TPayload> builder,
        Func<TPayload, TMessage> factory
    )
        where TMessage : IPersistedMessage<TMessage, TPayload>
        where TPayload : IPersistedMessagePayload<TPayload>
    {
        return builder.CreatedWithInternal(factory);
    }
}

public interface IPersistedMessageFinalBuilder;
