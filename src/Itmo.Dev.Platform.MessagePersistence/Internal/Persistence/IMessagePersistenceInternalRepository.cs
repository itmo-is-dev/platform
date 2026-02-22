using Itmo.Dev.Platform.MessagePersistence.Internal.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Persistence;

internal interface IMessagePersistenceInternalRepository
{
    IAsyncEnumerable<PersistedMessageModel> QueryAsync(InternalPersistedMessageQuery query, CancellationToken cancellationToken);

    IAsyncEnumerable<long> AddAsync(IReadOnlyCollection<PersistedMessageCreateModel> messages, CancellationToken cancellationToken);

    Task UpdateAsync(IReadOnlyCollection<PersistedMessageModel> messages, CancellationToken cancellationToken);
}