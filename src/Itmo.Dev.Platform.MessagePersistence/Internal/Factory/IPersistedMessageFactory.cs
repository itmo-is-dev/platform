using Itmo.Dev.Platform.MessagePersistence.Internal.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Factory;

internal interface IPersistedMessageFactory
{
    ValueTask<IPersistedMessage> CreateAsync(
        PersistedMessageModel message,
        CancellationToken cancellationToken);
}
