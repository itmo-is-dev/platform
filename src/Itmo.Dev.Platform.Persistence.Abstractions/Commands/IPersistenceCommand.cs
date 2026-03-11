using Newtonsoft.Json;
using System.Data.Common;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.Persistence.Abstractions.Commands;

public interface IPersistenceCommand : IAsyncDisposable
{
    ValueTask<DbDataReader> ExecuteReaderAsync(CancellationToken cancellationToken);

    ValueTask<int> ExecuteNonQueryAsync(CancellationToken cancellationToken);

    IPersistenceCommand AddParameter(DbParameter parameter);

    IPersistenceCommand AddParameter<T>(string parameterName, T value);

    [OverloadResolutionPriority(int.MaxValue)]
    IPersistenceCommand AddParameter<T>(string parameterName, IEnumerable<T> values);

    IPersistenceCommand AddMultiArrayStringParameter<T>(
        string parameterName,
        IEnumerable<IEnumerable<T>> values)
        where T : INumber<T>;

    IPersistenceCommand AddMultiArrayStringParameter(
        string parameterName,
        IEnumerable<IEnumerable<string>> values);

    IPersistenceCommand AddJsonParameter<T>(string parameterName, T value);

    IPersistenceCommand AddNullableJsonParameter<T>(string parameterName, T? value)
        where T : class;

    IPersistenceCommand AddJsonArrayParameter<T>(string parameterName, IEnumerable<T> values);

    IPersistenceCommand AddJsonArrayParameter(string parameterName, IEnumerable<string> values);
}
