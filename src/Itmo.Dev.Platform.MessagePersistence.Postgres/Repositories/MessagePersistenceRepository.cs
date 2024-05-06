using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Queries;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Repositories;

internal class MessagePersistenceRepository : IMessagePersistenceInternalRepository
{
    private readonly IPersistenceConnectionProvider _connectionProvider;
    private readonly MessagePersistenceQueryStorage _queryStorage;

    public MessagePersistenceRepository(
        IPersistenceConnectionProvider connectionProvider,
        MessagePersistenceQueryStorage queryStorage)
    {
        _connectionProvider = connectionProvider;
        _queryStorage = queryStorage;
    }

    public async IAsyncEnumerable<SerializedMessage> QueryAsync(
        SerializedMessageQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand(_queryStorage.Query.Value)
            .AddParameter("message_name", query.Name)
            .AddParameter("states", query.States)
            .AddParameter("ignore_cursor", query.Cursor is null)
            .AddParameter("cursor", query.Cursor ?? DateTimeOffset.MinValue)
            .AddParameter("page_size", query.PageSize);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        int id = reader.GetOrdinal("persisted_message_id");
        int name = reader.GetOrdinal("persisted_message_name");
        int createdAt = reader.GetOrdinal("persisted_message_created_at");
        int state = reader.GetOrdinal("persisted_message_state");
        int key = reader.GetOrdinal("persisted_message_key");
        int value = reader.GetOrdinal("persisted_message_value");
        int retryCount = reader.GetOrdinal("persisted_message_retry_count");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new SerializedMessage(
                Id: reader.GetInt64(id),
                Name: reader.GetString(name),
                CreatedAt: reader.GetFieldValue<DateTimeOffset>(createdAt),
                State: reader.GetFieldValue<MessageState>(state),
                Key: reader.GetString(key),
                Value: reader.GetString(value),
                RetryCount: reader.GetInt32(retryCount));
        }
    }

    public async Task AddAsync(IReadOnlyCollection<SerializedMessage> messages, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand(_queryStorage.Add.Value)
            .AddParameter("names", messages.Select(message => message.Name))
            .AddParameter("created_at", messages.Select(message => message.CreatedAt))
            .AddParameter("states", messages.Select(message => message.State))
            .AddJsonArrayParameter("keys", messages.Select(message => message.Key))
            .AddJsonArrayParameter("values", messages.Select(message => message.Value));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(IReadOnlyCollection<SerializedMessage> messages, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand(_queryStorage.UpdateStates.Value)
            .AddParameter("ids", messages.Select(x => x.Id))
            .AddParameter("states", messages.Select(x => x.State))
            .AddParameter("retry_counts", messages.Select(x => x.RetryCount));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}