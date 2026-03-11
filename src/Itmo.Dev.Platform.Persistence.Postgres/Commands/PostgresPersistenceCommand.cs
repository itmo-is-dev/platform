using Itmo.Dev.Platform.Common.Serialization;
using Itmo.Dev.Platform.Persistence.Abstractions.Commands;
using Itmo.Dev.Platform.Persistence.Postgres.Exceptions;
using Npgsql;
using NpgsqlTypes;
using System.Data.Common;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Itmo.Dev.Platform.Persistence.Postgres.Commands;

internal class PostgresPersistenceCommand : IPersistenceCommand
{
    private readonly NpgsqlCommand _command;
    private readonly IPlatformSerializer _serializer;

    public PostgresPersistenceCommand(NpgsqlCommand command, IPlatformSerializer serializer)
    {
        _command = command;
        _serializer = serializer;
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
        AddParameterCore(parameter);

        return this;
    }

    [OverloadResolutionPriority(int.MaxValue)]
    public IPersistenceCommand AddParameter<T>(string parameterName, IEnumerable<T> values)
    {
        var value = values is List<T> or T[] ? values : values.ToArray();

        var parameter = new NpgsqlParameter(parameterName: parameterName, value: value);
        AddParameterCore(parameter);

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

    public IPersistenceCommand AddJsonParameter<T>(string parameterName, T value)
    {
        var serialized = _serializer.Serialize(value, typeof(T)); 

        var parameter = new NpgsqlParameter(parameterName: parameterName, value: serialized)
        {
            NpgsqlDbType = NpgsqlDbType.Jsonb,
        };

        AddParameterCore(parameter);
        return this;
    }

    public IPersistenceCommand AddNullableJsonParameter<T>(string parameterName, T? value)
        where T : class
    {
        object serialized = value is null
            ? DBNull.Value
            : _serializer.Serialize(value, typeof(T));

        var parameter = new NpgsqlParameter(parameterName: parameterName, value: serialized)
        {
            NpgsqlDbType = NpgsqlDbType.Jsonb,
            IsNullable = true,
        };

        AddParameterCore(parameter);
        return this;
    }

    public IPersistenceCommand AddJsonArrayParameter<T>(string parameterName, IEnumerable<T> values)
    {
        var serialized = values
            .Select(value => _serializer.Serialize(value, typeof(T)))
            .ToArray();

        var parameter = new NpgsqlParameter(parameterName: parameterName, value: serialized)
        {
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            NpgsqlDbType = NpgsqlDbType.Jsonb | NpgsqlDbType.Array,
        };

        AddParameterCore(parameter);
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

        AddParameterCore(parameter);
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

        AddParameterCore(parameter);
        return;

        static string Serialize(IEnumerable<T> values)
        {
            var builder = new StringBuilder("{");
            builder.AppendJoin(", ", values);
            builder.Append('}');

            return builder.ToString();
        }
    }

    private void AddParameterCore(NpgsqlParameter parameter)
    {
        if (string.IsNullOrEmpty(parameter.ParameterName) is false
            && _command.Parameters.Any(x => x.ParameterName == parameter.ParameterName))
        {
            throw PlatformPersistencePostgresException.DuplicateParameter(parameter.ParameterName);
        }

        _command.Parameters.Add(parameter);
    }
}
