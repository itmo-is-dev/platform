using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Queries;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Itmo.Dev.Platform.Persistence.Postgres.Extensions;
using System.Data;
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
            .AddParameter("ids", query.Ids)
            .AddParameter("message_names", query.Names)
            .AddParameter("states", query.States)
            .AddParameter("ignore_cursor", query.Cursor is null)
            .AddParameter("cursor", query.Cursor ?? DateTimeOffset.MinValue)
            .AddParameter("page_size", query.PageSize);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new SerializedMessage(
                id: reader.GetInt64("persisted_message_id"),
                name: reader.GetString("persisted_message_name"),
                createdAt: reader.GetFieldValue<DateTimeOffset>("persisted_message_created_at"),
                state: reader.GetFieldValue<MessageState>("persisted_message_state"),
                key: reader.GetString("persisted_message_key"),
                value: reader.GetString("persisted_message_value"),
                retryCount: reader.GetInt32("persisted_message_retry_count"),
                bufferingStep: reader.GetNullableString("persisted_message_buffering_step"),
                headers: reader.GetJsonFieldValue<Dictionary<string, string>>("persisted_message_headers"));
        }
    }

    public async IAsyncEnumerable<long> AddAsync(
        IReadOnlyCollection<SerializedMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand(_queryStorage.Add.Value)
            .AddParameter("names", messages.Select(message => message.Name))
            .AddParameter("created_at", messages.Select(message => message.CreatedAt))
            .AddParameter("states", messages.Select(message => message.State))
            .AddJsonArrayParameter("keys", messages.Select(message => message.Key))
            .AddJsonArrayParameter("values", messages.Select(message => message.Value))
            .AddParameter("buffering_steps", messages.Select(message => message.BufferingStep))
            .AddJsonArrayParameter("headers", messages.Select(message => message.Headers));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return reader.GetInt64(0);
        }
    }

    public async Task UpdateAsync(IReadOnlyCollection<SerializedMessage> messages, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand(_queryStorage.Update.Value)
            .AddParameter("ids", messages.Select(message => message.Id))
            .AddParameter("states", messages.Select(message => message.State))
            .AddParameter("retry_counts", messages.Select(message => message.RetryCount))
            .AddParameter("buffering_steps", messages.Select(message => message.BufferingStep))
            .AddJsonArrayParameter("headers", messages.Select(message => message.Headers));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
