using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing.Proceeding;

internal class QueryParameterConfigurator : IQueryParameterConfigurator
{
    private readonly BackgroundTaskRunner _runner;

    public QueryParameterConfigurator(BackgroundTaskRunner runner)
    {
        _runner = runner;
    }

    public IExecutionMetadataConfigurator WithId(BackgroundTaskId backgroundTaskId)
    {
        var query = BackgroundTaskQuery.Build(builder => builder.WithId(backgroundTaskId));
        return new ExecutionMetadataConfigurator(_runner, query);
    }

    public IExecutionMetadataConfigurator WithQuery(BackgroundTaskQuery query)
    {
        return new ExecutionMetadataConfigurator(_runner, query);
    }
}