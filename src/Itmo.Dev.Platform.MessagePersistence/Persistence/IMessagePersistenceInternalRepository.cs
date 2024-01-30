using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Persistence;

internal interface IMessagePersistenceInternalRepository
{
    IAsyncEnumerable<SerializedMessage> QueryAsync(SerializedMessageQuery query, CancellationToken cancellationToken);

    Task AddAsync(IReadOnlyCollection<SerializedMessage> messages, CancellationToken cancellationToken);

    Task UpdateStatesAsync(MessageStateUpdateRequest request, CancellationToken cancellationToken);
}