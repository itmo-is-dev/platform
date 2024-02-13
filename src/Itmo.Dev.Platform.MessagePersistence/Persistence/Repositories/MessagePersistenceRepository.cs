using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Persistence.Queries;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.MessagePersistence.Persistence.Repositories;

internal class MessagePersistenceRepository : IMessagePersistenceInternalRepository
{
    private readonly IPostgresConnectionProvider _connectionProvider;
    private readonly MessagePersistenceQueryStorage _queryStorage;

    public MessagePersistenceRepository(
        IPostgresConnectionProvider connectionProvider,
        MessagePersistenceQueryStorage queryStorage)
    {
        _connectionProvider = connectionProvider;
        _queryStorage = queryStorage;
    }

    public async IAsyncEnumerable<SerializedMessage> QueryAsync(
        SerializedMessageQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

#pragma warning disable CA2100
        await using var command = new NpgsqlCommand(_queryStorage.Query.Value, connection)
            .AddParameter("message_name", query.Name)
            .AddParameter("states", query.States)
            .AddParameter("ignore_cursor", query.Cursor is null)
            .AddParameter("cursor", query.Cursor ?? DateTimeOffset.MinValue)
            .AddParameter("page_size", query.PageSize);
#pragma warning restore CA2100

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
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

#pragma warning disable CA2100
        await using var command = new NpgsqlCommand(_queryStorage.Add.Value, connection)
            .AddParameter("names", messages.Select(message => message.Name))
            .AddParameter("created_at", messages.Select(message => message.CreatedAt))
            .AddParameter("states", messages.Select(message => message.State))
            .AddJsonArrayParameter("keys", messages.Select(message => message.Key))
            .AddJsonArrayParameter("values", messages.Select(message => message.Value));
#pragma warning restore CA2100

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(IReadOnlyCollection<SerializedMessage> messages, CancellationToken cancellationToken)
    {
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

#pragma warning disable CA2100
        await using var command = new NpgsqlCommand(_queryStorage.UpdateStates.Value, connection)
            .AddParameter("ids", messages.Select(x => x.Id))
            .AddParameter("states", messages.Select(x => x.State))
            .AddParameter("retry_counts", messages.Select(x => x.RetryCount));
#pragma warning restore CA2100

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}