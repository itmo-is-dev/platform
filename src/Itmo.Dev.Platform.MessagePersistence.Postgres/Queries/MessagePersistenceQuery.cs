using Itmo.Dev.Platform.MessagePersistence.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Queries;

internal class MessagePersistenceQuery : IDisposable
{
    private readonly IDisposable? _disposable;

    public MessagePersistenceQuery(
        Func<MessagePersistencePostgresOptions, string> factory,
        IOptionsMonitor<MessagePersistencePostgresOptions> monitor)
    {
        _disposable = monitor.OnChange(o => Value = factory.Invoke(o));
        Value = factory.Invoke(monitor.CurrentValue);
    }

    public string Value { get; private set; }

    public void Dispose()
    {
        _disposable?.Dispose();
    }
}