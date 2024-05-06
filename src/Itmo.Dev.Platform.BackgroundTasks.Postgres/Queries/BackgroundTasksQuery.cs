using Itmo.Dev.Platform.BackgroundTasks.Postgres.Configuration;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Postgres.Queries;

internal class BackgroundTasksQuery : IDisposable
{
    private readonly IDisposable? _disposable;

    public BackgroundTasksQuery(
        Func<BackgroundTaskPersistenceOptions, string> factory,
        IOptionsMonitor<BackgroundTaskPersistenceOptions> monitor)
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