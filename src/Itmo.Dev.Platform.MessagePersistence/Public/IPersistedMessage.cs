using Itmo.Dev.Platform.Common.Attributes;

namespace Itmo.Dev.Platform.MessagePersistence;

[PlatformImplementationOnly("implement IPersistedMessage<>")]
public interface IPersistedMessage
{
    IPersistedMessagePayload Payload { get; }
}

[PlatformImplementationOnly("implement IPersistedMessage<>")]
public interface IPersistedMessage<TSelf> : IPersistedMessage
    where TSelf : IPersistedMessage
{
    static abstract IPersistedMessageFinalBuilder Configure(
        IPersistedMessageBuilder<TSelf> builder);
}

public interface IPersistedMessage<TSelf, TPayload> : IPersistedMessage<TSelf>
    where TSelf : IPersistedMessage<TSelf, TPayload>
    where TPayload : IPersistedMessagePayload<TPayload>
{
    IPersistedMessagePayload IPersistedMessage.Payload => Payload;

    new TPayload Payload { get; }
}
