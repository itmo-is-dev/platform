using Itmo.Dev.Platform.MessagePersistence.Configuration;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Persistence.Queries;

internal class MessagePersistenceQuery : IDisposable
{
    private readonly IDisposable? _disposable;

    public MessagePersistenceQuery(
        Func<MessagePersistencePersistenceOptions, string> factory,
        IOptionsMonitor<MessagePersistencePersistenceOptions> monitor)
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