using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Itmo.Dev.Platform.MessagePersistence.Internal.Persistence;
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

    public async IAsyncEnumerable<PersistedMessageModel> QueryAsync(
        PersistedMessageQuery query,
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
            yield return new PersistedMessageModel
            {
                Id = reader.GetInt64("persisted_message_id"),
                Name = reader.GetString("persisted_message_name"),
                Version = reader.GetInt64("persisted_message_version"),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>("persisted_message_created_at"),
                State = reader.GetFieldValue<MessageState>("persisted_message_state"),
                Key = reader.GetString("persisted_message_key"),
                Value = reader.GetString("persisted_message_value"),
                RetryCount = reader.GetInt32("persisted_message_retry_count"),
                BufferingStep = reader.GetNullableString("persisted_message_buffering_step"),
                Headers = reader.GetJsonFieldValue<Dictionary<string, string>>("persisted_message_headers"),
            };
        }
    }

    public async IAsyncEnumerable<long> AddAsync(
        IReadOnlyCollection<PersistedMessageCreateModel> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand(_queryStorage.Add.Value)
            .AddParameter("names", messages.Select(message => message.Name))
            .AddParameter("versions", messages.Select(message => message.Version.Value))
            .AddParameter("created_at", messages.Select(message => message.CreatedAt))
            .AddParameter("states", messages.Select(message => message.State))
            .AddJsonArrayParameter("keys", messages.Select(message => message.Key))
            .AddJsonArrayParameter("values", messages.Select(message => message.Value))
            .AddJsonArrayParameter("headers", messages.Select(message => message.Headers));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return reader.GetInt64(0);
        }
    }

    public async Task UpdateAsync(
        IReadOnlyCollection<PersistedMessageModel> messages,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand(_queryStorage.Update.Value)
            .AddParameter("ids", messages.Select(message => message.Id))
            .AddParameter("versions", messages.Select(message => message.Version.Value))
            .AddParameter("states", messages.Select(message => message.State))
            .AddParameter("retry_counts", messages.Select(message => message.RetryCount))
            .AddParameter("buffering_steps", messages.Select(message => message.BufferingStep))
            .AddJsonArrayParameter("headers", messages.Select(message => message.Headers));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
