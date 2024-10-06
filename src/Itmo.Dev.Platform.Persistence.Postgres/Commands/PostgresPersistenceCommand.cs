using Itmo.Dev.Platform.Persistence.Abstractions.Commands;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System.Data.Common;
using System.Numerics;
using System.Text;

namespace Itmo.Dev.Platform.Persistence.Postgres.Commands;

internal class PostgresPersistenceCommand : IPersistenceCommand
{
    private readonly NpgsqlCommand _command;

    public PostgresPersistenceCommand(NpgsqlCommand command)
    {
        _command = command;
    }

    public async ValueTask<DbDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
    {
        return await _command.ExecuteReaderAsync(cancellationToken);
    }

    public async ValueTask<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        return await _command.ExecuteNonQueryAsync(cancellationToken);
    }

    public IPersistenceCommand AddParameter(DbParameter parameter)
    {
        if (parameter is not NpgsqlParameter npgsqlParameter)
            throw new InvalidOperationException($"Cannot use {parameter.GetType()} with Postgres connection");

        _command.Parameters.Add(npgsqlParameter);

        return this;
    }

    public IPersistenceCommand AddParameter<T>(string parameterName, T value)
    {
        var parameter = new NpgsqlParameter<T>(parameterName: parameterName, value: value);
        _command.Parameters.Add(parameter);

        return this;
    }

    public IPersistenceCommand AddParameter<T>(string parameterName, IEnumerable<T> values)
    {
        var value = values is List<T> or T[] ? values : values.ToArray();

        var parameter = new NpgsqlParameter(parameterName: parameterName, value: value);
        _command.Parameters.Add(parameter);

        return this;
    }

    public IPersistenceCommand AddMultiArrayStringParameter<T>(string parameterName, IEnumerable<IEnumerable<T>> values)
        where T : INumber<T>
    {
        AddMultiArrayStringParameterCore(parameterName, values);
        return this;
    }

    public IPersistenceCommand AddMultiArrayStringParameter(
        string parameterName,
        IEnumerable<IEnumerable<string>> values)
    {
        AddMultiArrayStringParameterCore(parameterName, values);
        return this;
    }

    public IPersistenceCommand AddJsonParameter<T>(
        string parameterName,
        T value,
        JsonSerializerSettings? serializerSettings = null)
    {
        var serialized = JsonConvert.SerializeObject(value, serializerSettings);

        var parameter = new NpgsqlParameter(parameterName: parameterName, value: serialized)
        {
            NpgsqlDbType = NpgsqlDbType.Jsonb,
        };

        _command.Parameters.Add(parameter);

        return this;
    }

    public IPersistenceCommand AddNullableJsonParameter<T>(
        string parameterName,
        T? value,
        JsonSerializerSettings? serializerSettings = null)
        where T : class
    {
        object serialized = value is null ? DBNull.Value : JsonConvert.SerializeObject(value, serializerSettings);

        var parameter = new NpgsqlParameter(parameterName: parameterName, value: serialized)
        {
            NpgsqlDbType = NpgsqlDbType.Jsonb,
            IsNullable = true,
        };

        _command.Parameters.Add(parameter);

        return this;
    }

    public IPersistenceCommand AddJsonArrayParameter<T>(
        string parameterName,
        IEnumerable<T> values,
        JsonSerializerSettings? serializerSettings = null)
    {
        var serialized = values
            .Select(value => JsonConvert.SerializeObject(value, serializerSettings))
            .ToArray();

        var parameter = new NpgsqlParameter(parameterName: parameterName, value: serialized)
        {
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            NpgsqlDbType = NpgsqlDbType.Jsonb | NpgsqlDbType.Array,
        };

        _command.Parameters.Add(parameter);

        return this;
    }

    public IPersistenceCommand AddJsonArrayParameter(string parameterName, IEnumerable<string> values)
    {
        var value = values is List<string> or string[] ? values : values.ToArray();

        var parameter = new NpgsqlParameter(parameterName: parameterName, value: value)
        {
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            NpgsqlDbType = NpgsqlDbType.Jsonb | NpgsqlDbType.Array,
        };

        _command.Parameters.Add(parameter);

        return this;
    }

    public async ValueTask DisposeAsync()
    {
        await _command.DisposeAsync();
    }

    private void AddMultiArrayStringParameterCore<T>(
        string parameterName,
        IEnumerable<IEnumerable<T>> values)
    {
        var serialized = values
            .Select(Serialize)
            .ToArray();

        var parameter = new NpgsqlParameter(parameterName: parameterName, value: serialized)
        {
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Text,
        };

        _command.Parameters.Add(parameter);
        return;

        static string Serialize(IEnumerable<T> values)
        {
            var builder = new StringBuilder("{");
            builder.AppendJoin(", ", values);
            builder.Append('}');

            return builder.ToString();
        }
    }
}