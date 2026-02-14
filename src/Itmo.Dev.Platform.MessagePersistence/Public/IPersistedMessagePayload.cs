using Itmo.Dev.Platform.Common.Attributes;

namespace Itmo.Dev.Platform.MessagePersistence;

[PlatformImplementationOnly("implement IPersistedMessagePayload<>")]
public interface IPersistedMessagePayload
{
    object Key { get; }
}

public interface IPersistedMessagePayload<TSelf> : IPersistedMessagePayload
    where TSelf : IPersistedMessagePayload<TSelf>;

public interface IPersistedMessagePayload<TSelf, TPrevious> : IPersistedMessagePayload<TSelf>
    where TSelf : IPersistedMessagePayload<TSelf, TPrevious>
    where TPrevious : IPersistedMessagePayload<TPrevious>
{
    static abstract ValueTask<TSelf> MigrateAsync(
        TPrevious previous,
        IServiceProvider provider,
        CancellationToken cancellationToken);
}
