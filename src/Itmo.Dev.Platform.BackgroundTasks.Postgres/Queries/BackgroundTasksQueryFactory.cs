using Itmo.Dev.Platform.BackgroundTasks.Postgres.Configuration;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Postgres.Queries;

internal class BackgroundTasksQueryFactory
{
    private readonly IOptionsMonitor<BackgroundTaskPersistenceOptions> _options;

    public BackgroundTasksQueryFactory(IOptionsMonitor<BackgroundTaskPersistenceOptions> options)
    {
        _options = options;
    }

    public BackgroundTasksQuery Create(Func<BackgroundTaskPersistenceOptions, string> factory)
    {
        return new BackgroundTasksQuery(factory, _options);
    }
}