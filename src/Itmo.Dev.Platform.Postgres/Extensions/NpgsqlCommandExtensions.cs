using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System.Numerics;
using System.Text;

namespace Itmo.Dev.Platform.Postgres.Extensions;

public static class NpgsqlCommandExtensions
{
    public static NpgsqlBatchCommand ToBatchCommand(this NpgsqlCommand command)
    {
        var batchCommand = new NpgsqlBatchCommand(command.CommandText);

        foreach (NpgsqlParameter parameter in command.Parameters)
        {
            parameter.Collection = null;
            batchCommand.Parameters.Add(parameter);
        }

        return batchCommand;
    }

    public static NpgsqlCommand AddParameter<T>(this NpgsqlCommand command, string parameterName, T value)
    {
        var parameter = new NpgsqlParameter<T>(parameterName: parameterName, value: value);
        command.Parameters.Add(parameter);

        return command;
    }

    public static NpgsqlCommand AddMultiArrayStringParameter<T>(
        this NpgsqlCommand command,
        string parameterName,
        IEnumerable<IEnumerable<T>> values)
        where T : INumber<T>
    {
        return command.AddMultiArrayStringParameterCore(parameterName, values);
    }

    public static NpgsqlCommand AddMultiArrayStringParameter(
        this NpgsqlCommand command,
        string parameterName,
        IEnumerable<IEnumerable<string>> values)
    {
        return command.AddMultiArrayStringParameterCore(parameterName, values);
    }

    public static NpgsqlCommand AddJsonParameter<T>(
        this NpgsqlCommand command,
        string parameterName,
        T value,
        JsonSerializerSettings? serializerSettings = null)
    {
        var serialized = JsonConvert.SerializeObject(value, serializerSettings);

        var parameter = new NpgsqlParameter(parameterName: parameterName, value: serialized)
        {
            NpgsqlDbType = NpgsqlDbType.Jsonb,
        };

        command.Parameters.Add(parameter);

        return command;
    }

    public static NpgsqlCommand AddNullableJsonParameter<T>(
        this NpgsqlCommand command,
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

        command.Parameters.Add(parameter);

        return command;
    }

    public static NpgsqlCommand AddJsonArrayParameter<T>(
        this NpgsqlCommand command,
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

        command.Parameters.Add(parameter);

        return command;
    }

    private static NpgsqlCommand AddMultiArrayStringParameterCore<T>(
        this NpgsqlCommand command,
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

        command.Parameters.Add(parameter);

        return command;

        static string Serialize(IEnumerable<T> values)
        {
            var builder = new StringBuilder("{");
            builder.AppendJoin(", ", values);
            builder.Append('}');

            return builder.ToString();
        }
    }
}