using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System.Numerics;
using System.Text;

namespace Itmo.Dev.Platform.Postgres.Extensions;

public static class NpgsqlCommandExtensions
{
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
}