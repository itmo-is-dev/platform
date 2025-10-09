using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Persistence;

internal interface IMessagePersistenceInternalRepository
{
    IAsyncEnumerable<SerializedMessage> QueryAsync(SerializedMessageQuery query, CancellationToken cancellationToken);

    IAsyncEnumerable<long> AddAsync(IReadOnlyCollection<SerializedMessage> messages, CancellationToken cancellationToken);

    Task UpdateAsync(IReadOnlyCollection<SerializedMessage> messages, CancellationToken cancellationToken);
}